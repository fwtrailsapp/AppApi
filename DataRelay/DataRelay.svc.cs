﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.ServiceModel;

namespace DataRelay
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class DataRelay : IDataRelay
    {
        private static Logger _log;

        public DataRelay()
        {
            _log = new Logger(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }
        
        public int CreateNewAccount(string username, string password, int dob, int weight, string sex, int height)
        {
            _log.WriteTraceLine(this, string.Format("Creating new account: {0}", username));

            try
            {
                string connectionString = ConfigurationManager.AppSettings["connectionString"];

                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                    sqlConn.Open();

                    if (accountExists(sqlConn, username))
                    {
                        _log.WriteTraceLine(this, string.Format("Account '{0}' already exists!", username));
                        return 401;
                    }

                    string createAcctQuery = "INSERT INTO ACCOUNT (accountID, username, password, dob, weight, sex, height) VALUES (@accountID, @username, @password, @dob, @weight, @sex, @height)";
                    
                    string accountID = Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty).Substring(0, 20);

                    using (SqlCommand cmdCreateAcct = new SqlCommand(createAcctQuery, sqlConn))
                    {
                        cmdCreateAcct.Parameters.AddWithValue("@accountID", accountID);

                        cmdCreateAcct.Parameters.AddWithValue("@username", username);
                        cmdCreateAcct.Parameters.AddWithValue("@password", password);

                        if (!dob.Equals(null))
                            cmdCreateAcct.Parameters.AddWithValue("@dob", dob);
                        else
                            cmdCreateAcct.Parameters.AddWithValue("@dob", DBNull.Value);

                        cmdCreateAcct.Parameters.AddWithValue("@weight", weight);
                        cmdCreateAcct.Parameters.AddWithValue("@sex", sex);
                        cmdCreateAcct.Parameters.AddWithValue("@height", height);
                        
                        int result = cmdCreateAcct.ExecuteNonQuery();

                        if (result == 1)
                        {
                            _log.WriteTraceLine(this, string.Format("Account '{0}' created successfully!", username));
                            return 200;
                        }
                        else
                        {
                            _log.WriteTraceLine(this, string.Format("Account '{0}' was not created!", username));
                            return 402;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteErrorLog(ex.GetType(), ex);
            }

            return 500;
        }

        public string[] GetAccountInfo(string username)
        {
            _log.WriteTraceLine(this, string.Format("Getting account information for '{0}'.", username));

            string connectionString = ConfigurationManager.AppSettings["connectionString"];

            int dob = -1, weight = -1, height =-1;
            string sex = string.Empty;

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                    sqlConn.Open();

                    string getUserInfo = "SELECT TOP 1 * FROM ACCOUNT WHERE [username]=@username";

                    using (SqlCommand cmdGetUserInfo = new SqlCommand(getUserInfo, sqlConn))
                    {
                        cmdGetUserInfo.Parameters.AddWithValue("@username", username);
                        
                        using (SqlDataReader reader = cmdGetUserInfo.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                
                                if (!reader["dob"].Equals(DBNull.Value))
                                    dob = reader.GetInt32(reader.GetOrdinal("dob"));

                                if (!reader["weight"].Equals(DBNull.Value))
                                    weight = reader.GetInt32(reader.GetOrdinal("weight"));

                                if (!reader["sex"].Equals(DBNull.Value))
                                    sex = reader.GetString(reader.GetOrdinal("sex"));

                                if (!reader["height"].Equals(DBNull.Value))
                                    height = reader.GetInt32(reader.GetOrdinal("height"));
                            }
                            reader.Close();
                        }
                    }
                    sqlConn.Close();
                }
            }
            catch (Exception ex)
            {
                _log.WriteErrorLog(ex.GetType(), ex);
            }
            
            string[] response = new string[5] { "200", dob.ToString(), weight.ToString(), sex, height.ToString() };

            return response;
        }

        public int CreateNewActivity(string username, string time_started, string duration, float mileage, int calories_burned, string exercise_type, string path)
        {
            _log.WriteTraceLine(this, string.Format("Creating a new activity for '{0}'!", username));

            try
            {
                string connectionString = ConfigurationManager.AppSettings["connectionString"];
                int result = -1;

                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                    sqlConn.Open();

                    string acctGuid = string.Empty;

                    if (!accountExists(sqlConn, username))
                    {
                        _log.WriteTraceLine(this, string.Format("Account '{0}' does not exist!", username));
                        return 401;
                    }
                    else
                    {
                        acctGuid = getAccountGuid(sqlConn, username);
                    }

                    #region -- CREATE ACTIVITY --
                    int exerciseLookupId = getExerciseTypeID(sqlConn, exercise_type);
                    DateTime timeStamp = Convert.ToDateTime(time_started);
                    TimeSpan ActTime = TimeSpan.Parse(duration);

                    string createActivityQuery = "INSERT INTO ACTIVITY (accountUserID, exerciseType, startTime, duration, distance, caloriesBurned) VALUES (@accountUserID, @exerciseType, @startTime, @duration, @distance, @caloriesBurned)";

                    using (SqlCommand cmdCreateActivity = new SqlCommand(createActivityQuery, sqlConn))
                    {
                        cmdCreateActivity.Parameters.AddWithValue("@accountUserID", acctGuid);
                        cmdCreateActivity.Parameters.AddWithValue("@exerciseType", exerciseLookupId);
                        cmdCreateActivity.Parameters.AddWithValue("@startTime", timeStamp);
                        cmdCreateActivity.Parameters.AddWithValue("@duration", ActTime.TotalSeconds);
                        cmdCreateActivity.Parameters.AddWithValue("@distance", mileage);
                        cmdCreateActivity.Parameters.AddWithValue("@caloriesBurned", calories_burned);

                        result = cmdCreateActivity.ExecuteNonQuery();

                        if (result != 1)
                        {
                            _log.WriteTraceLine(this, string.Format("Activity could not be created for '{0}'!", username));
                            return 500;
                        }
                    }

                    #endregion -- CREATE ACTIVITY --

                    #region -- GET ACTIVITYID --

                    int activityID = -1;

                    string getActivityIDQuery = "SELECT TOP 1 [activityID] FROM ACTIVITY WHERE accountUserID=@accountUserID AND exerciseType=@exerciseType AND startTime=@startTime AND duration=@duration AND distance=@distance AND caloriesBurned=@caloriesBurned";

                    using (SqlCommand cmdGetActivityID = new SqlCommand(getActivityIDQuery, sqlConn))
                    {
                        cmdGetActivityID.Parameters.AddWithValue("@accountUserID", acctGuid);
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
                                _log.WriteTraceLine(this, string.Format("ActivityID from new Activity for '{0}' could not be retrieved!", username));
                                return 500;
                            }
                        }

                        if (activityID == -1)
                        {
                            _log.WriteTraceLine(this, string.Format("Unknown error occured upon retreiving ActivityID for '{0}'!", username));
                            return 501;
                        }
                    }

                    #endregion -- GET ACTIVITYID --

                    #region -- CREATE PATH SEGMENT --

                    string createPathSegmentQuery = "INSERT INTO [PathSegment] (activityID, path) VALUES (@activityID, @path)";

                    using (SqlCommand cmdCreatePathSegmentQuery = new SqlCommand(createPathSegmentQuery, sqlConn))
                    {
                        cmdCreatePathSegmentQuery.Parameters.AddWithValue("@activityID", activityID);
                        cmdCreatePathSegmentQuery.Parameters.AddWithValue("@path", path);

                        result = cmdCreatePathSegmentQuery.ExecuteNonQuery();

                        if (result != 1)
                        {
                            _log.WriteTraceLine(this, string.Format("Failed to create PathSegment record for '{0}'!", username));
                            return 500;
                        }
                    }

                    #endregion -- CREATE PATH SEGMENT --

                    sqlConn.Close();
                }

                _log.WriteTraceLine(this, string.Format("Activity succesfully created for '{0}'!", username));

                return 200;
            }
            catch (Exception ex)
            {
                _log.WriteErrorLog(ex.GetType(), ex);
            }

            return 500;
        }

        public Activity[] GetActivitiesForUser(string username)
        {
            _log.WriteTraceLine(this, string.Format("Retreiving all activities for user '{0}'", username));

            List<Activity> activities = null;

            try
            {
                string connectionString = ConfigurationManager.AppSettings["connectionString"];

                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                    sqlConn.Open();

                    string accountUserID = string.Empty;

                    if (!accountExists(sqlConn, username))
                    {
                        _log.WriteTraceLine(this, string.Format("Account '{0}' does not exist!", username));
                        return null;
                    }
                    else
                    {
                        accountUserID = getAccountGuid(sqlConn, username);
                    }

                    activities = new List<Activity>();

                    string getAllActivity = "SELECT E.exerciseDescription as exerciseType, A.startTime, A.duration, A.distance, A.caloriesBurned FROM Activity A JOIN exerciseType E on A.exerciseType = E.lookupCode WHERE A.[accountUserID] = @accountUserID";

                    using (SqlCommand cmdGetAllActivity = new SqlCommand(getAllActivity, sqlConn))
                    {
                        cmdGetAllActivity.Parameters.AddWithValue("@accountUserID", accountUserID);

                        using (SqlDataReader reader = cmdGetAllActivity.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while(reader.Read())
                                {
                                    Activity a = new Activity();

                                    a.time_started = reader.GetDateTime(reader.GetOrdinal("startTime")).ToString("yyyy-MM-dd'T'hh:mm:ss");
                                    a.duration = TimeSpan.FromSeconds(reader.GetInt32(reader.GetOrdinal("duration"))).ToString();
                                    a.mileage = (float)reader.GetDouble(reader.GetOrdinal("distance"));
                                    a.calories_burned = reader.GetInt32(reader.GetOrdinal("caloriesBurned"));
                                    a.exercise_type = reader.GetString(reader.GetOrdinal("exerciseType"));

                                    activities.Add(a);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteErrorLog(ex.GetType(), ex);
            }

            return activities.ToArray();
        }

        public TotalStat[] GetTotalStatsForUser(string username)
        {
            _log.WriteTraceLine(this, string.Format("Retreiving all activities for user '{0}'", username));

            List<Activity> activities = null;
            List<TotalStat> totalstats = null;

            try
            {
                string connectionString = ConfigurationManager.AppSettings["connectionString"];

                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                    sqlConn.Open();

                    string accountUserID = string.Empty;

                    if (!accountExists(sqlConn, username))
                    {
                        _log.WriteTraceLine(this, string.Format("Account '{0}' does not exist!", username));
                        return null;
                    }
                    else
                    {
                        accountUserID = getAccountGuid(sqlConn, username);
                    }

                    activities = new List<Activity>();
                    totalstats = new List<TotalStat>();

                    string getAllActivity = "SELECT E.exerciseDescription as exerciseType, A.startTime, A.duration, A.distance, A.caloriesBurned FROM Activity A JOIN exerciseType E on A.exerciseType = E.lookupCode WHERE A.[accountUserID] = @accountUserID";

                    using (SqlCommand cmdGetAllActivity = new SqlCommand(getAllActivity, sqlConn))
                    {
                        cmdGetAllActivity.Parameters.AddWithValue("@accountUserID", accountUserID);

                        using (SqlDataReader reader = cmdGetAllActivity.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                TotalStat overall = new TotalStat();
                                TotalStat bike = new TotalStat();
                                TotalStat run = new TotalStat();
                                TotalStat walk = new TotalStat();
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
                                    else if(a.exercise_type.Equals("walk"))
                                    {
                                        middleIntWalk += reader.GetInt32(reader.GetOrdinal("duration"));
                                        walk.total_duration = TimeSpan.FromSeconds(middleIntWalk).ToString();
                                        walk.total_distance += a.mileage;
                                        walk.total_calories += a.calories_burned;

                                    }

                                    activities.Add(a);
                                        
                                }
                                totalstats.Add(overall);
                                totalstats.Add(bike);
                                totalstats.Add(run);
                                totalstats.Add(walk);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteErrorLog(ex.GetType(), ex);
            }

            return totalstats.ToArray();
        }

        public Path[] GetPath()
        {
            _log.WriteTraceLine(this, string.Format("Retreiving all paths"));

            List<Path> pathArray = null;

            try
            {
                string connectionString = ConfigurationManager.AppSettings["connectionString"];

                using (SqlConnection sqlConn = new SqlConnection(connectionString))
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
                _log.WriteErrorLog(ex.GetType(), ex);
            }

            return pathArray.ToArray();
        }

    }
}
