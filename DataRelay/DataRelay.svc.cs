using System;
using System.Configuration;
using System.Data.SqlClient;
using System.ServiceModel;

namespace DataRelay
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.PerCall)]
    public class DataRelay : IDataRelay
    {
        private static Logger _log;

        public DataRelay()
        {
            _log = new Logger(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }
        
        public int CreateNewAccount(string username, string password, int dob, int weight, string sex, int height)
        {
            string connectionString = ConfigurationManager.AppSettings["connectionString"];

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                    sqlConn.Open();

                    string queryExists = "SELECT COUNT(*) FROM ACCOUNT WHERE [username]=@username";

                    using (SqlCommand cmdQueryExists = new SqlCommand(queryExists, sqlConn))
                    {
                        cmdQueryExists.Parameters.AddWithValue("@username", username);

                        int acctCount = 0;
                        Int32.TryParse(cmdQueryExists.ExecuteScalar().ToString(), out acctCount);

                        if (acctCount > 0)
                        {
                            return 401;
                        }
                    }

                    string createAcctQuery = "INSERT INTO ACCOUNT (accountID, username, password, dob, weight, sex, height) VALUES (@accountID, @username, @password, @dob, @weight, @sex, @height)";
                    
                    string accountID = Guid.NewGuid().ToString().Replace("-", string.Empty).Replace("+", string.Empty).Substring(0, 10);

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
                            return 200;
                        }
                        else
                        {
                            return 401;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteErrorLog(ex.GetType(), ex);
            }

            return 401;
        }

        public string[] GetAccountInfo(string username)
        {
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
    }
}
