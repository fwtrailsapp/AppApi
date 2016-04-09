using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace DataRelay
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.PerCall)]
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

        public void CreateNewAccount(string username, string password, int? birthyear, int? weight, string sex, int? height)
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

                    string createAcctQuery = "INSERT INTO ACCOUNT (accountID, username, password, birthyear, weight, sex, height) VALUES (@accountID, @username, @password, @birthyear, @weight, @sex, @height)";
                    
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

                    string editAccount = "UPDATE ACCOUNT SET username=@username, password=@password, birthyear=@birthyear, weight=@weight, sex=@sex, height=@height WHERE accountID=@accountID";

                    using (SqlCommand cmdEditAcct = new SqlCommand(editAccount, sqlConn))
                    {
                        cmdEditAcct.Parameters.AddWithValue("@accountID", RequestAccountId);
                        cmdEditAcct.Parameters.AddWithValue("@username", username);
                        cmdEditAcct.Parameters.AddWithValue("@password", PasswordStorage.Hash(password));

                        if (!birthyear.Equals(null))
                            cmdEditAcct.Parameters.AddWithValue("@birthyear", birthyear);
                        else
                            cmdEditAcct.Parameters.AddWithValue("@birthyear", DBNull.Value);

                        if (!weight.Equals(null))
                            cmdEditAcct.Parameters.AddWithValue("@weight", weight);
                        else
                            cmdEditAcct.Parameters.AddWithValue("@weight", DBNull.Value);

                        if (!sex.Equals(null))
                            cmdEditAcct.Parameters.AddWithValue("@sex", sex);
                        else
                            cmdEditAcct.Parameters.AddWithValue("@sex", DBNull.Value);

                        if(!height.Equals(null))
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
                        throw new WebFaultException<string>("Username or password is incorrect.", HttpStatusCode.Unauthorized);
                    }

                    //check password
                    var userHashedPassword = GetAccountHash(sqlConn, username);
                    if (!PasswordStorage.PasswordMatch(password, userHashedPassword))
                    {
                        _log.WriteTraceLine(this, $"Account '{username}' specified the wrong password!");
                        throw new WebFaultException<string>("Username or password is incorrect.", HttpStatusCode.Unauthorized);
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
                            throw new WebFaultException<string>("Couldn't get account info.", HttpStatusCode.InternalServerError);
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

        public void CreateNewActivity(string time_started, string duration, float mileage, int calories_burned, string exercise_type, string path)
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

                    string createActivityQuery = "INSERT INTO ACTIVITY (accountUserID, exerciseType, startTime, duration, distance, caloriesBurned) VALUES (@accountUserID, @exerciseType, @startTime, @duration, @distance, @caloriesBurned)";

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
                            throw new WebFaultException<string>("Activity could not be created.", HttpStatusCode.InternalServerError);
                        }
                    }

                    #endregion -- CREATE ACTIVITY --

                    #region -- GET ACTIVITYID --

                    int activityID;

                    string getActivityIDQuery = "SELECT TOP 1 [activityID] FROM ACTIVITY WHERE accountUserID=@accountUserID AND exerciseType=@exerciseType AND startTime=@startTime AND duration=@duration AND distance=@distance AND caloriesBurned=@caloriesBurned";

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
                                throw new WebFaultException<string>("Activity could not be created.", HttpStatusCode.InternalServerError);
                            }
                        }

                        if (activityID == -1)
                        {
                            _log.WriteTraceLine(this,
                                $"Unknown error occured upon retreiving ActivityID for '{RequestAccountId}'!");
                            throw new WebFaultException<string>("Activity could not be created.", HttpStatusCode.InternalServerError);
                        }
                    }

                    #endregion -- GET ACTIVITYID --

                    #region -- CREATE PATH SEGMENT --

                    string createPathSegmentQuery = "INSERT INTO [PathSegment] (activityID, path) VALUES (@activityID, @path)";

                    using (SqlCommand cmdCreatePathSegmentQuery = new SqlCommand(createPathSegmentQuery, sqlConn))
                    {
                        cmdCreatePathSegmentQuery.Parameters.AddWithValue("@activityID", activityID);
                        cmdCreatePathSegmentQuery.Parameters.AddWithValue("@path", path);

                        if (cmdCreatePathSegmentQuery.ExecuteNonQuery() != 1)
                        {
                            _log.WriteTraceLine(this, $"Failed to create PathSegment record for '{RequestAccountId}'!");
                            throw new WebFaultException<string>("Activity could not be created.", HttpStatusCode.InternalServerError);
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

                    string getAllActivity = "SELECT E.exerciseDescription as exerciseType, A.startTime, A.duration, A.distance, A.caloriesBurned " +
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
                                while(reader.Read())
                                {
                                    var timeStarted = reader.GetDateTime(reader.GetOrdinal("startTime")).ToString("yyyy-MM-dd'T'hh:mm:ss");
                                    var duration = TimeSpan.FromSeconds(reader.GetInt32(reader.GetOrdinal("duration"))).ToString();

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

                    const string getAllActivity = "SELECT E.exerciseDescription as exerciseType, A.startTime, A.duration, A.distance, A.caloriesBurned " +
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
                                    durationSecondsBike += (int)TimeSpan.Parse(a.duration).TotalSeconds;
                                    mileageBike += a.mileage;
                                    caloriesBike += a.calories_burned;

                                }
                                else if (a.exercise_type.Equals("run"))
                                {
                                    durationSecondsRun += (int)TimeSpan.Parse(a.duration).TotalSeconds;
                                    mileageRun += a.mileage;
                                    caloriesRun += a.calories_burned;

                                }
                                else if (a.exercise_type.Equals("walk"))
                                {
                                    durationSecondsWalk += (int)TimeSpan.Parse(a.duration).TotalSeconds;
                                    mileageWalk += a.mileage;
                                    caloriesWalk += a.calories_burned;

                                }
                            }

                            var allStats = new[]
                            {
                                new TotalStat("Overall", TimeSpan.FromSeconds(durationSecondsOverall).ToString(), mileageOverall, caloriesOverall), 
                                new TotalStat("Bike", TimeSpan.FromSeconds(durationSecondsBike).ToString(), mileageBike, caloriesBike), 
                                new TotalStat("Run", TimeSpan.FromSeconds(durationSecondsRun).ToString(), mileageRun, caloriesRun), 
                                new TotalStat("Walk", TimeSpan.FromSeconds(durationSecondsWalk).ToString(), mileageWalk, caloriesWalk)
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

                    string getAllActivity = "SELECT E.exerciseDescription as exerciseType, A.startTime, A.duration, A.distance, A.caloriesBurned FROM Activity A JOIN exerciseType E on A.exerciseType = E.lookupCode";

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
                                    a.mileage = (float)reader.GetDouble(reader.GetOrdinal("distance"));
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
    }
}
