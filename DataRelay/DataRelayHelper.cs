using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;

namespace DataRelay
{
    public partial class DataRelay
    {
        /// <summary>
        /// Returns true if an account with the specified username already exists, false otherwise.
        /// </summary>
        /// <param name="sqlConn">the database connection to use</param>
        /// <param name="username">the username of which to determine the availability</param>
        /// <returns></returns>
        private bool accountExists(SqlConnection sqlConn, string username)
        {
            const string queryExists = "SELECT COUNT(*) FROM [Account] WHERE [username]=@username";

            using (var cmdQueryExists = new SqlCommand(queryExists, sqlConn))
            {
                cmdQueryExists.Parameters.AddWithValue("@username", username);

                int acctCount;
                int.TryParse(cmdQueryExists.ExecuteScalar().ToString(), out acctCount);

                return acctCount == 1;
            }
        }

        /// <summary>
        /// Get the password hash for an account given its username.
        /// </summary>
        /// <param name="sqlConn">the database connection to use</param>
        /// <param name="username">the username of the account to lookup</param>
        private string GetAccountHash(SqlConnection sqlConn, string username)
        {
            const string queryHash = "SELECT TOP 1 [password] FROM [Account] WHERE [username]=@username";

            using (var cmdQueryHash = new SqlCommand(queryHash, sqlConn))
            {
                cmdQueryHash.Parameters.AddWithValue("@username", username);

                using (var reader = cmdQueryHash.ExecuteReader())
                {
                    if (!reader.HasRows) return string.Empty;
                    if (!reader.Read()) return string.Empty;
                    return reader.GetString(reader.GetOrdinal("password"));
                }
            }
        }

        /// <summary>
        /// Retrieve an account's ID given its username.
        /// </summary>
        /// <param name="sqlConn">the database connection to use</param>
        /// <param name="username">the username of the account to lookup</param>
        private static string GetAccountGuid(SqlConnection sqlConn, string username)
        {
            const string queryGetGuid = "SELECT TOP 1 [accountID] FROM [Account] where [username]=@username";

            using (var cmdQueryGetGuid = new SqlCommand(queryGetGuid, sqlConn))
            {
                cmdQueryGetGuid.Parameters.AddWithValue("username", username);

                using (var reader = cmdQueryGetGuid.ExecuteReader())
                {
                    if (!reader.HasRows) return string.Empty;
                    if (!reader.Read()) return string.Empty;
                    if (reader["accountID"].Equals(DBNull.Value)) return string.Empty;
                    return reader.GetString(reader.GetOrdinal("accountID"));
                }
            }
        }

        /// <summary>
        /// Gets username associated with an account ID
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <param name="accountID"></param>
        /// <returns></returns>
        private static string GetAccountUsername(SqlConnection sqlConn, string accountID)
        {
            const string queryGetAccountUsername = "SELECT TOP 1 [username] From [Account] WHERE [accountID]=@accountID";

            using (var cmdQueryGetAccountUsername = new SqlCommand(queryGetAccountUsername, sqlConn))
            {
                cmdQueryGetAccountUsername.Parameters.AddWithValue("accountID", accountID);

                using (var reader = cmdQueryGetAccountUsername.ExecuteReader())
                {
                    if (!reader.HasRows) return string.Empty;
                    if (!reader.Read()) return string.Empty;
                    if (reader["username"].Equals(DBNull.Value)) return string.Empty;
                    return reader.GetString(reader.GetOrdinal("username"));
                }
            }
        }
        /// <summary>
        /// Get the exercise type ID associated with the <see cref="exercise_type"/>.
        /// </summary>
        /// <param name="sqlConn">the database connection to use</param>
        /// <param name="exercise_type">the excersize type to lookup: "walk", "bike", or "run"</param>
        public int getExerciseTypeID(SqlConnection sqlConn, string exercise_type)
        {
            int lookUpID = -1;

            string exerciseIDLook = "SELECT TOP 1 [lookupCode] FROM [exerciseType] where [exerciseDescription]=@exerciseDescription";

            using (SqlCommand cmdExerciseLook = new SqlCommand(exerciseIDLook, sqlConn))
            {
                cmdExerciseLook.Parameters.AddWithValue("exerciseDescription", exercise_type);

                using (SqlDataReader reader = cmdExerciseLook.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (!reader["lookupCode"].Equals(DBNull.Value))
                            lookUpID = reader.GetInt32(reader.GetOrdinal("lookupCode"));
                    }
                }
            }

            return lookUpID;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <param name="report_type"></param>
        /// <returns></returns>
        public int getReportTypeID(SqlConnection sqlConn, string report_type)
        {
            int lookUpID = -1;

            string reportIDLook = "SELECT TOP 1 [code] FROM [TicketType] where [description]=@description";

            using (SqlCommand cmdReportLook = new SqlCommand(reportIDLook, sqlConn))
            {
                cmdReportLook.Parameters.AddWithValue("description", report_type);

                using (SqlDataReader reader = cmdReportLook.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (!reader["code"].Equals(DBNull.Value))
                            lookUpID = reader.GetInt32(reader.GetOrdinal("code"));
                    }
                }
            }

            return lookUpID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <param name="report_type"></param>
        /// <returns></returns>
        public string getReportColor(SqlConnection sqlConn, string report_type)
        {
            string color = null;

            string reportColorLook = "SELECT color From TicketType where description=@description";

            using (SqlCommand cmdColorLook = new SqlCommand(reportColorLook, sqlConn))
            {
                cmdColorLook.Parameters.AddWithValue("@description", report_type);

                using (SqlDataReader reader = cmdColorLook.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (!reader["color"].Equals(DBNull.Value))
                        {
                            color = reader.GetString(reader.GetOrdinal("color"));
                        }
                    }
                }

                
            }
            return color;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GenerateAccountGuid()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty).Substring(0, 20);
        }

        /// <summary>
        /// Using the <see cref="AccountSessionManager"/>, get the account ID from the request's login token.
        /// </summary>
        /// <returns>the accountID if the user is logged in, null otherwise</returns>
        private string RequestAccountId
        {
            get
            {
                var tokenStr = WebOperationContext.Current?.IncomingRequest.Headers["Trails-Api-Key"];
                if (tokenStr == null)
                    return null;

                Guid g;
                if (!Guid.TryParse(tokenStr, out g))
                    return null;

                var token = new LoginToken(g);

                try
                {
                    return _sessionManager.GetAccountIdFromToken(token);
                }
                catch (KeyNotFoundException)
                {
                    //the account hasn't logged in
                    return null;
                }
            }
        }

        /// <summary>
        /// Throws an HTTP 401 if the client is not logged in. After calling, the <see cref="RequestAccountId"/> is guaranteed to be non-null.
        /// </summary>
        private void RequireLoginToken()
        {
            if (RequestAccountId == null)
            {
                _log.WriteDebugLog($"Login token was not sent or was invalid.");
                throw new WebFaultException<string>("Login token was not sent or was invalid.", HttpStatusCode.Unauthorized);
            }
        }

        /// <summary>
        /// The database connection string
        /// </summary>
        private static string ConnectionString => ConfigurationManager.AppSettings["connectionString"];
        
        /// <summary>
        /// Throw the exception as a HTTP 500 if it isn't already some other HTTP code.
        /// </summary>
        /// <param name="ex">the exception to make into a <see cref="WebFaultException"/></param>
        /// <param name="detail">the error message to accopany the HTTP response</param>
        private void GenericErrorHandler(Exception ex, string detail)
        {
            _log.WriteErrorLog(ex.GetType(), ex);
            if (ex is WebFaultException<string>)
                throw ex;
            throw new WebFaultException<string>(detail, HttpStatusCode.InternalServerError);
        }
    }
}