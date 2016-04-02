using System;
using System.Configuration;
using System.Data.SqlClient;
using System.ServiceModel;

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

        public string getAccountGuid(SqlConnection sqlConn, string username)
        {
            string guid = string.Empty;

            string queryGetGuid = "SELECT TOP 1 [accountID] FROM [Account] where [username]=@username";

            using (SqlCommand cmdQueryGetGuid = new SqlCommand(queryGetGuid, sqlConn))
            {
                cmdQueryGetGuid.Parameters.AddWithValue("username", username);

                using (SqlDataReader reader = cmdQueryGetGuid.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (!reader["accountID"].Equals(DBNull.Value))
                            guid = reader.GetString(reader.GetOrdinal("accountID"));
                    }
                }
            }

                return guid;
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
    }
}