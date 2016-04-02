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
        public Boolean accountExists(SqlConnection sqlConn, string username)
        {
            string queryExists = "SELECT COUNT(*) FROM [Account] WHERE [username]=@username";

            using (SqlCommand cmdQueryExists = new SqlCommand(queryExists, sqlConn))
            {
                cmdQueryExists.Parameters.AddWithValue("@username", username);

                int acctCount = 0;
                Int32.TryParse(cmdQueryExists.ExecuteScalar().ToString(), out acctCount);

                if (acctCount != 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

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

        private static string GenerateAccountGuid()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty).Substring(0, 20);
        }

        private string RequestAccountId
        {
            get
            {
                var tokenStr = HttpContext.Current.Request.Headers["Trails-Api-Key"];
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

        private void RequireLoginToken()
        {
            if (RequestAccountId == null)
            {
                _log.WriteDebugLog("Client did not specifiy a login token.");
                throw new WebFaultException<string>("Login token was not sent or was invalid.", HttpStatusCode.Unauthorized);
            }
        }
    }
}