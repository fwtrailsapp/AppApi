using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace DataRelay
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true,
        InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class DataRelay : IDataRelay
    {
        private static Logger _log;
        private static AccountSessionManager _sessionManager;
        public DataRelay()
        {
            if (_log == null)
                _log = new Logger(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            if (_sessionManager == null)
                _sessionManager = new AccountSessionManager();
        }

        public void CreateNewAccount(string username, string password, int? birthyear, int? weight, string sex,
            int? height)
        {
            _log.WriteTraceLine(this, $"Creating new account: {username}");

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    if (accountExists(sqlConn, username))
                    {
                        _log.WriteTraceLine(this, $"Account '{username}' already exists!");
                        throw new WebFaultException<string>("Account already exists", HttpStatusCode.Conflict);
                    }

                    string createAcctQuery =
                        "INSERT INTO ACCOUNT (accountID, username, password, birthyear, weight, sex, height) VALUES (@accountID, @username, @password, @birthyear, @weight, @sex, @height)";

                    string accountID = GenerateAccountGuid();

                    using (SqlCommand cmdCreateAcct = new SqlCommand(createAcctQuery, sqlConn))
                    {
                        cmdCreateAcct.Parameters.AddWithValue("@accountID", accountID);

                        cmdCreateAcct.Parameters.AddWithValue("@username", username);
                        cmdCreateAcct.Parameters.AddWithValue("@password", PasswordStorage.Hash(password));

                        if (birthyear != null)
                            cmdCreateAcct.Parameters.AddWithValue("@birthyear", birthyear);
                        else
                            cmdCreateAcct.Parameters.AddWithValue("@birthyear", DBNull.Value);

                        if (weight != null)
                            cmdCreateAcct.Parameters.AddWithValue("@weight", weight);
                        else
                            cmdCreateAcct.Parameters.AddWithValue("@weight", DBNull.Value);

                        if (sex != null)
                            cmdCreateAcct.Parameters.AddWithValue("@sex", sex);
                        else
                            cmdCreateAcct.Parameters.AddWithValue("@sex", DBNull.Value);

                        if (height != null)
                            cmdCreateAcct.Parameters.AddWithValue("@height", height);
                        else
                            cmdCreateAcct.Parameters.AddWithValue("@height", DBNull.Value);

                        int result = cmdCreateAcct.ExecuteNonQuery();

                        if (result != 1)
                        {
                            _log.WriteTraceLine(this, $"Account '{username}' was not created!");
                            throw new WebFaultException<string>("Account couldn't be created.",
                                HttpStatusCode.InternalServerError);
                        }

                        _log.WriteTraceLine(this, $"Account '{username}' created successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Account couldn't be created.");
            }
        }

        public void EditAccount(string username, string password, int? birthyear, int? weight, string sex, int? height)
        {
            _log.WriteTraceLine(this, $"Updating account");
            RequireLoginToken();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    string editAccount =
                        "UPDATE ACCOUNT SET username=@username, password=@password, birthyear=@birthyear, weight=@weight, sex=@sex, height=@height WHERE accountID=@accountID";

                    using (SqlCommand cmdEditAcct = new SqlCommand(editAccount, sqlConn))
                    {
                        cmdEditAcct.Parameters.AddWithValue("@accountID", RequestAccountId);
                        cmdEditAcct.Parameters.AddWithValue("@username", username);
                        cmdEditAcct.Parameters.AddWithValue("@password", PasswordStorage.Hash(password));

                        if (birthyear != null)
                            cmdEditAcct.Parameters.AddWithValue("@birthyear", birthyear);
                        else
                            cmdEditAcct.Parameters.AddWithValue("@birthyear", DBNull.Value);

                        if (weight != null)
                            cmdEditAcct.Parameters.AddWithValue("@weight", weight);
                        else
                            cmdEditAcct.Parameters.AddWithValue("@weight", DBNull.Value);

                        if (sex != null)
                            cmdEditAcct.Parameters.AddWithValue("@sex", sex);
                        else
                            cmdEditAcct.Parameters.AddWithValue("@sex", DBNull.Value);

                        if (height != null)
                            cmdEditAcct.Parameters.AddWithValue("@height", height);
                        else
                            cmdEditAcct.Parameters.AddWithValue("@height", DBNull.Value);

                        int result = cmdEditAcct.ExecuteNonQuery();

                        if (result != 1)
                        {
                            _log.WriteTraceLine(this, $"Account was not updated!");
                            throw new WebFaultException<string>("Account couldn't be updated 1.",
                                HttpStatusCode.InternalServerError);
                        }

                        _log.WriteTraceLine(this, $"Account successfully updated 1");
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Account couldn't be updated 2.");
            }
        }

        public LoginToken Login(string username, string password)
        {
            _log.WriteTraceLine(this, $"Logging in an account: {username}");
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    //find the account's guid
                    var acctGuid = GetAccountGuid(sqlConn, username);
                    if (acctGuid == string.Empty)
                    {
                        _log.WriteTraceLine(this, $"Account '{username}' does not exist!");
                        throw new WebFaultException<string>("Username or password is incorrect.",
                            HttpStatusCode.Unauthorized);
                    }

                    //check password
                    var userHashedPassword = GetAccountHash(sqlConn, username);
                    if (!PasswordStorage.PasswordMatch(password, userHashedPassword))
                    {
                        _log.WriteTraceLine(this, $"Account '{username}' specified the wrong password!");
                        throw new WebFaultException<string>("Username or password is incorrect.",
                            HttpStatusCode.Unauthorized);
                    }

                    var token = _sessionManager.Add(acctGuid);
                    return token;
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Account couldn't be logged in.");
                return null; //will never be hit because genericerrorhandler always throws
            }
        }

        public Account GetAccountInfo()
        {
            _log.WriteTraceLine(this, $"Getting account information for '{RequestAccountId}'.");
            RequireLoginToken();
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    string getUserInfo = "SELECT TOP 1 * FROM ACCOUNT WHERE [AccountId]=@accountId";

                    using (SqlCommand cmdGetUserInfo = new SqlCommand(getUserInfo, sqlConn))
                    {
                        cmdGetUserInfo.Parameters.AddWithValue("@accountId", RequestAccountId);

                        using (SqlDataReader reader = cmdGetUserInfo.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                Account account = new Account();
                                reader.Read();

                                if (!reader["birthyear"].Equals(DBNull.Value))
                                    account.birthyear = reader.GetInt32(reader.GetOrdinal("birthyear"));

                                if (!reader["weight"].Equals(DBNull.Value))
                                    account.weight = reader.GetInt32(reader.GetOrdinal("weight"));

                                if (!reader["sex"].Equals(DBNull.Value))
                                    account.sex = reader.GetString(reader.GetOrdinal("sex"));

                                if (!reader["height"].Equals(DBNull.Value))
                                    account.height = reader.GetInt32(reader.GetOrdinal("height"));

                                return account;
                            }
                            throw new WebFaultException<string>("Couldn't get account info.",
                                HttpStatusCode.InternalServerError);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Couldn't get account info.");
                return null; //will never be hit because genericerrorhandler always throws
            }
        }

        public void CreateNewActivity(string time_started, string duration, float mileage, int calories_burned,
            string exercise_type, string path)
        {
            _log.WriteTraceLine(this, $"Creating a new activity for '{RequestAccountId}'!");
            RequireLoginToken();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    #region -- CREATE ACTIVITY --

                    int exerciseLookupId = getExerciseTypeID(sqlConn, exercise_type);
                    DateTime timeStamp = Convert.ToDateTime(time_started);
                    TimeSpan ActTime = TimeSpan.Parse(duration);

                    string createActivityQuery =
                        "INSERT INTO ACTIVITY (accountUserID, exerciseType, startTime, duration, distance, caloriesBurned) VALUES (@accountUserID, @exerciseType, @startTime, @duration, @distance, @caloriesBurned)";

                    using (SqlCommand cmdCreateActivity = new SqlCommand(createActivityQuery, sqlConn))
                    {
                        cmdCreateActivity.Parameters.AddWithValue("@accountUserID", RequestAccountId);
                        cmdCreateActivity.Parameters.AddWithValue("@exerciseType", exerciseLookupId);
                        cmdCreateActivity.Parameters.AddWithValue("@startTime", timeStamp);
                        cmdCreateActivity.Parameters.AddWithValue("@duration", ActTime.TotalSeconds);
                        cmdCreateActivity.Parameters.AddWithValue("@distance", mileage);
                        cmdCreateActivity.Parameters.AddWithValue("@caloriesBurned", calories_burned);

                        if (cmdCreateActivity.ExecuteNonQuery() != 1)
                        {
                            _log.WriteTraceLine(this, $"Activity could not be created for '{RequestAccountId}'!");
                            throw new WebFaultException<string>("Activity could not be created.",
                                HttpStatusCode.InternalServerError);
                        }
                    }

                    #endregion -- CREATE ACTIVITY --

                    #region -- GET ACTIVITYID --

                    int activityID;

                    string getActivityIDQuery =
                        "SELECT TOP 1 [activityID] FROM ACTIVITY WHERE accountUserID=@accountUserID AND exerciseType=@exerciseType AND startTime=@startTime AND duration=@duration AND distance=@distance AND caloriesBurned=@caloriesBurned";

                    using (SqlCommand cmdGetActivityID = new SqlCommand(getActivityIDQuery, sqlConn))
                    {
                        cmdGetActivityID.Parameters.AddWithValue("@accountUserID", RequestAccountId);
                        cmdGetActivityID.Parameters.AddWithValue("@exerciseType", exerciseLookupId);
                        cmdGetActivityID.Parameters.AddWithValue("@startTime", timeStamp);
                        cmdGetActivityID.Parameters.AddWithValue("@duration", ActTime.TotalSeconds);
                        cmdGetActivityID.Parameters.AddWithValue("@distance", mileage);
                        cmdGetActivityID.Parameters.AddWithValue("@caloriesBurned", calories_burned);

                        using (SqlDataReader reader = cmdGetActivityID.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                activityID = reader.GetInt32(reader.GetOrdinal("activityID"));
                            }
                            else
                            {
                                _log.WriteTraceLine(this,
                                    $"ActivityID from new Activity for '{RequestAccountId}' could not be retrieved!");
                                throw new WebFaultException<string>("Activity could not be created.",
                                    HttpStatusCode.InternalServerError);
                            }
                        }

                        if (activityID == -1)
                        {
                            _log.WriteTraceLine(this,
                                $"Unknown error occured upon retreiving ActivityID for '{RequestAccountId}'!");
                            throw new WebFaultException<string>("Activity could not be created.",
                                HttpStatusCode.InternalServerError);
                        }
                    }

                    #endregion -- GET ACTIVITYID --

                    #region -- CREATE PATH SEGMENT --

                    string createPathSegmentQuery =
                        "INSERT INTO [PathSegment] (activityID, path) VALUES (@activityID, @path)";

                    using (SqlCommand cmdCreatePathSegmentQuery = new SqlCommand(createPathSegmentQuery, sqlConn))
                    {
                        cmdCreatePathSegmentQuery.Parameters.AddWithValue("@activityID", activityID);
                        cmdCreatePathSegmentQuery.Parameters.AddWithValue("@path", path);

                        if (cmdCreatePathSegmentQuery.ExecuteNonQuery() != 1)
                        {
                            _log.WriteTraceLine(this, $"Failed to create PathSegment record for '{RequestAccountId}'!");
                            throw new WebFaultException<string>("Activity could not be created.",
                                HttpStatusCode.InternalServerError);
                        }
                    }

                    #endregion -- CREATE PATH SEGMENT --
                }

                _log.WriteTraceLine(this, $"Activity succesfully created for '{RequestAccountId}'!");
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Activity could not be created.");
            }
        }
        public void CreateNewTicket(string type, string description, int? active, string imgLink, double latitude,
            double longitude, string title, string date, string username, string notes, string dateClosed)
        {
            _log.WriteTraceLine(this, $"Creating new ticket: {title}");

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    DateTime dateOpen = DateTime.ParseExact(date, "yyyy-MM-dd'T'HH:mm:ss", null);

                    string createTicketQuery = "INSERT INTO Ticket (Type, Description, Active, ImgLink, Latitude, Longitude, Title, Date, Username, Notes, TypeColor, DateClosed) VALUES (@type, @description, @active, @imgLink, @latitude,"
                        +" @longitude, @title, @date, @username, @notes, @color, @dateClosed)";

                    
                    using (SqlCommand cmdCreateTicket = new SqlCommand(createTicketQuery, sqlConn))
                    {
                        if (imgLink == "")
                            cmdCreateTicket.Parameters.AddWithValue("@imgLink", DBNull.Value);
                        else
                        {
                            byte[] bytes = Convert.FromBase64String(imgLink);
                            string filepath = "C:\\images\\img_" + latitude.ToString() + longitude.ToString() +".jpg";
                            Image image;

                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                image = Image.FromStream(ms);
                                image.Save(filepath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                            cmdCreateTicket.Parameters.AddWithValue("@imgLink", filepath);
                        }
                        cmdCreateTicket.Parameters.AddWithValue("@type", type);
                        cmdCreateTicket.Parameters.AddWithValue("@description", description);
                        cmdCreateTicket.Parameters.AddWithValue("@active", active);
                        cmdCreateTicket.Parameters.AddWithValue("@latitude", latitude);
                        cmdCreateTicket.Parameters.AddWithValue("@longitude", longitude);
                        cmdCreateTicket.Parameters.AddWithValue("@title", title);
                        cmdCreateTicket.Parameters.AddWithValue("@date", dateOpen);
                        cmdCreateTicket.Parameters.AddWithValue("@username", username);
                        cmdCreateTicket.Parameters.AddWithValue("@notes", notes);
                        cmdCreateTicket.Parameters.AddWithValue("@color", getReportColor(sqlConn, type));
                        cmdCreateTicket.Parameters.AddWithValue("@dateClosed", dateClosed);

                        int result = cmdCreateTicket.ExecuteNonQuery();

                        if (result != 1)
                        {
                            _log.WriteTraceLine(this, $"Ticket '{title}' was not created!");
                            throw new WebFaultException<string>("Ticket couldn't be created.",
                                HttpStatusCode.InternalServerError);
                        }

                        _log.WriteTraceLine(this, $"Ticket '{title}' created successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Could not create ticket");
            }
        }

        public void CloseTicket(int id)
        {
            _log.WriteTraceLine(this, $"Closing ticket number {id}");
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    string closeQuery = "Update Ticket Set active = 0, dateClosed = @dateClosed Where id=@id";

                    using (SqlCommand cmdClose = new SqlCommand(closeQuery, sqlConn))
                    {
                        cmdClose.Parameters.AddWithValue("@id", id);
                        cmdClose.Parameters.AddWithValue("@dateClosed", DateTime.Now);
                        cmdClose.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Couldn't close ticket");
            }
        }
        
        public Activity[] GetActivitiesForUser()
        {
            _log.WriteTraceLine(this, $"Retreiving all activities for user '{RequestAccountId}'");
            RequireLoginToken();

            List<Activity> activities = null;

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    activities = new List<Activity>();

                    string getAllActivity =
                        "SELECT E.exerciseDescription as exerciseType, A.startTime, A.duration, A.distance, A.caloriesBurned " +
                        "FROM Activity A JOIN exerciseType E on A.exerciseType = E.lookupCode " +
                        "WHERE A.[accountUserID] = @accountUserID " +
                        "ORDER BY A.startTime DESC";

                    using (SqlCommand cmdGetAllActivity = new SqlCommand(getAllActivity, sqlConn))
                    {
                        cmdGetAllActivity.Parameters.AddWithValue("@accountUserID", RequestAccountId);

                        using (SqlDataReader reader = cmdGetAllActivity.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var timeStarted =
                                        reader.GetDateTime(reader.GetOrdinal("startTime"))
                                            .ToString("yyyy-MM-dd'T'hh:mm:ss");
                                    var duration =
                                        TimeSpan.FromSeconds(reader.GetInt32(reader.GetOrdinal("duration"))).ToString();

                                    var a = new Activity
                                    {
                                        time_started = timeStarted,
                                        duration = duration,
                                        mileage = (float) reader.GetDouble(reader.GetOrdinal("distance")),
                                        calories_burned = reader.GetInt32(reader.GetOrdinal("caloriesBurned")),
                                        exercise_type = reader.GetString(reader.GetOrdinal("exerciseType"))
                                    };

                                    activities.Add(a);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Activities could not be retrieved.");
            }

            return activities.ToArray();
        }

        public TotalStat[] GetTotalStatsForUser()
        {
            _log.WriteTraceLine(this, $"Retreiving all statistics for user '{RequestAccountId}'");
            RequireLoginToken();

            try
            {
                using (var sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    const string getAllActivity =
                        "SELECT E.exerciseDescription as exerciseType, A.startTime, A.duration, A.distance, A.caloriesBurned " +
                        "FROM Activity A JOIN exerciseType E on A.exerciseType = E.lookupCode " +
                        "WHERE A.[accountUserID] = @accountUserID";

                    using (var cmdGetAllActivity = new SqlCommand(getAllActivity, sqlConn))
                    {
                        cmdGetAllActivity.Parameters.AddWithValue("@accountUserID", RequestAccountId);

                        using (var reader = cmdGetAllActivity.ExecuteReader())
                        {
                            var activities = new List<Activity>();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var seconds = reader.GetInt32(reader.GetOrdinal("duration"));
                                    var timespan = TimeSpan.FromSeconds(seconds);
                                    var strDuration = timespan.ToString();

                                    var a = new Activity
                                    {
                                        duration = strDuration,
                                        mileage = (float) reader.GetDouble(reader.GetOrdinal("distance")),
                                        calories_burned = reader.GetInt32(reader.GetOrdinal("caloriesBurned")),
                                        exercise_type = reader.GetString(reader.GetOrdinal("exerciseType"))
                                    };

                                    activities.Add(a);
                                }
                            }

                            var durationSecondsOverall = 0;
                            var durationSecondsBike = 0;
                            var durationSecondsRun = 0;
                            var durationSecondsWalk = 0;

                            var mileageOverall = 0f;
                            var mileageBike = 0f;
                            var mileageRun = 0f;
                            var mileageWalk = 0f;

                            var caloriesOverall = 0;
                            var caloriesBike = 0;
                            var caloriesRun = 0;
                            var caloriesWalk = 0;

                            foreach (var a in activities)
                            {
                                durationSecondsOverall += (int) TimeSpan.Parse(a.duration).TotalSeconds;
                                mileageOverall += a.mileage;
                                caloriesOverall += a.calories_burned;

                                if (a.exercise_type.Equals("bike"))
                                {
                                    durationSecondsBike += (int) TimeSpan.Parse(a.duration).TotalSeconds;
                                    mileageBike += a.mileage;
                                    caloriesBike += a.calories_burned;
                                }
                                else if (a.exercise_type.Equals("run"))
                                {
                                    durationSecondsRun += (int) TimeSpan.Parse(a.duration).TotalSeconds;
                                    mileageRun += a.mileage;
                                    caloriesRun += a.calories_burned;
                                }
                                else if (a.exercise_type.Equals("walk"))
                                {
                                    durationSecondsWalk += (int) TimeSpan.Parse(a.duration).TotalSeconds;
                                    mileageWalk += a.mileage;
                                    caloriesWalk += a.calories_burned;
                                }
                            }

                            var allStats = new[]
                            {
                                new TotalStat("Overall", TimeSpan.FromSeconds(durationSecondsOverall).ToString(),
                                    mileageOverall, caloriesOverall),
                                new TotalStat("Bike", TimeSpan.FromSeconds(durationSecondsBike).ToString(), mileageBike,
                                    caloriesBike),
                                new TotalStat("Run", TimeSpan.FromSeconds(durationSecondsRun).ToString(), mileageRun,
                                    caloriesRun),
                                new TotalStat("Walk", TimeSpan.FromSeconds(durationSecondsWalk).ToString(), mileageWalk,
                                    caloriesWalk)
                            };

                            return allStats;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Couldn't get total stats.");
                return null;
            }
        }

        public Path[] GetPath()
        {
            _log.WriteTraceLine(this, string.Format("Retreiving all paths"));

            List<Path> pathArray = null;

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    pathArray = new List<Path>();

                    string getAllPath = "SELECT * FROM PathSegment";

                    using (SqlCommand cmdGetAllPath = new SqlCommand(getAllPath, sqlConn))
                    {
                        using (SqlDataReader reader = cmdGetAllPath.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Path a = new Path();
                                    a.path = reader.GetString(reader.GetOrdinal("path"));
                                    pathArray.Add(a);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Couldn't get all paths");
            }

            return pathArray.ToArray();
        }

        public AllStat[] GetAllStats()
        {
            _log.WriteTraceLine(this, string.Format("Retreiving all activity stats"));

            List<Activity> activities = null;
            List<AllStat> allStat = null;

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    activities = new List<Activity>();
                    allStat = new List<AllStat>();

                    string getAllActivity =
                        "SELECT E.exerciseDescription as exerciseType, A.startTime, A.duration, A.distance, A.caloriesBurned FROM Activity A JOIN exerciseType E on A.exerciseType = E.lookupCode";

                    using (SqlCommand cmdGetAllActivity = new SqlCommand(getAllActivity, sqlConn))
                    {
                        using (SqlDataReader reader = cmdGetAllActivity.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                AllStat overall = new AllStat();
                                AllStat bike = new AllStat();
                                AllStat run = new AllStat();
                                AllStat walk = new AllStat();
                                int middleInt = 0;
                                int middleIntBike = 0;
                                int middleIntRun = 0;
                                int middleIntWalk = 0;

                                overall.type = "Overall";
                                bike.type = "Bike";
                                run.type = "Run";
                                walk.type = "Walk";

                                while (reader.Read())
                                {
                                    Activity a = new Activity();
                                    a.duration = reader.GetInt32(reader.GetOrdinal("duration")).ToString();
                                    a.mileage = (float) reader.GetDouble(reader.GetOrdinal("distance"));
                                    a.calories_burned = reader.GetInt32(reader.GetOrdinal("caloriesBurned"));
                                    a.exercise_type = reader.GetString(reader.GetOrdinal("exerciseType"));

                                    middleInt += reader.GetInt32(reader.GetOrdinal("duration"));


                                    overall.total_duration = TimeSpan.FromSeconds(middleInt).ToString();
                                    overall.total_distance += a.mileage;
                                    overall.total_calories += a.calories_burned;

                                    if (a.exercise_type.Equals("bike"))
                                    {
                                        middleIntBike += reader.GetInt32(reader.GetOrdinal("duration"));
                                        bike.total_duration = TimeSpan.FromSeconds(middleIntBike).ToString();
                                        bike.total_distance += a.mileage;
                                        bike.total_calories += a.calories_burned;
                                    }
                                    else if (a.exercise_type.Equals("run"))
                                    {
                                        middleIntRun += reader.GetInt32(reader.GetOrdinal("duration"));
                                        run.total_duration = TimeSpan.FromSeconds(middleIntRun).ToString();
                                        run.total_distance += a.mileage;
                                        run.total_calories += a.calories_burned;
                                    }
                                    else if (a.exercise_type.Equals("walk"))
                                    {
                                        middleIntWalk += reader.GetInt32(reader.GetOrdinal("duration"));
                                        walk.total_duration = TimeSpan.FromSeconds(middleIntWalk).ToString();
                                        walk.total_distance += a.mileage;
                                        walk.total_calories += a.calories_burned;
                                    }

                                    activities.Add(a);
                                }
                                allStat.Add(overall);
                                allStat.Add(bike);
                                allStat.Add(run);
                                allStat.Add(walk);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteErrorLog(ex.GetType(), ex);
            }

            return allStat.ToArray();
        }

        public Ticket[] GetTickets()
        {
            _log.WriteTraceLine(this, $"Retreiving all tickets");
            RequireLoginToken();

            List<Ticket> tickets = null;

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    tickets = new List<Ticket>();

                    string getAllTickets = "SELECT * From Tickets Order By date DESC";

                    using (SqlCommand cmdGetAllActivity = new SqlCommand(getAllTickets, sqlConn))
                    {
                        using (SqlDataReader reader = cmdGetAllActivity.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var a = new Ticket
                                    {
                                        title = reader.GetString(reader.GetOrdinal("title")),
                                        description = reader.GetString(reader.GetOrdinal("description")),
                                        latitude = reader.GetDouble(reader.GetOrdinal("latitude")),
                                        longitude = reader.GetDouble(reader.GetOrdinal("longitude")),
                                        imgLink = reader.GetString(reader.GetOrdinal("imageLink")),
                                        date = reader.GetString(reader.GetOrdinal("date")),
                                        type = reader.GetString(reader.GetOrdinal("type")),
                                        color = reader.GetString(reader.GetOrdinal("color")),
                                    };

                                    tickets.Add(a);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Activities could not be retrieved.");
            }

            return tickets.ToArray();
        }

        public string getImageLink(int id)
        {
            string imageLink = "";
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    string getImageLinkQuery = "Select imgLink from Ticket Where id=@id";

                    using (SqlCommand cmdGetImage = new SqlCommand(getImageLinkQuery, sqlConn))
                    {
                        cmdGetImage.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = cmdGetImage.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();

                                imageLink = reader.GetString(reader.GetOrdinal("ImgLink"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Couldn't get image link.");
            }

            return imageLink;
        }

        public string getGPS(int id)
        {
            string gps = "";
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    string getGPSQuery = "Select latitude, longitude From Ticket Where id=@id";

                    using (SqlCommand cmdGetGPS = new SqlCommand(getGPSQuery, sqlConn))
                    {
                        cmdGetGPS.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = cmdGetGPS.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();

                                gps = reader.GetDouble(reader.GetOrdinal("Latitude")).ToString() + ", " + reader.GetDouble(reader.GetOrdinal("Longitude")).ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Couldn't retrieve gps coordinates.");
            }
            return gps;
        }

        public string getNote(int id)
        {
            string notes = "";
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    string getNoteQuery = "Select Notes From Ticket Where id=@id";

                    using (SqlCommand cmdGetNotes = new SqlCommand(getNoteQuery, sqlConn))
                    {
                        cmdGetNotes.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = cmdGetNotes.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();

                                notes = reader.GetString(reader.GetOrdinal("Notes"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Couldn't retrieve ticket notes.");
            }
            return notes;
        }

        public void setNotes(int id, string notes)
        {
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    string setNotesQuery = "Update Ticket Set Notes=@notes Where id=@id";

                    using (SqlCommand cmdSetNotes = new SqlCommand(setNotesQuery, sqlConn))
                    {
                        cmdSetNotes.Parameters.AddWithValue("@id", id);
                        cmdSetNotes.Parameters.AddWithValue("@notes", notes);

                        int result = cmdSetNotes.ExecuteNonQuery();

                        if (result != 1)
                        {
                            _log.WriteTraceLine(this, $"Notes '{notes}' was not set!");
                            throw new WebFaultException<string>("Notes couldn't be set.",
                                HttpStatusCode.InternalServerError);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Couldn't set notes for ticket.");
            }
        }

        public string getTicketTitle(int id)
        {
            string title = "";
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                {
                    sqlConn.Open();

                    string getTitleQuery = "Select title From Ticket Where id=@id";

                    using (SqlCommand cmdGetTitle = new SqlCommand(getTitleQuery, sqlConn))
                    {
                        cmdGetTitle.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = cmdGetTitle.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();

                                title = reader.GetString(reader.GetOrdinal("Title"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GenericErrorHandler(ex, "Couldn't retrieve gps coordinates.");
            }
            return title;
        }

    }
}