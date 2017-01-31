using System.ServiceModel;
using System.ServiceModel.Web;

namespace DataRelay
{
    [ServiceContract]
    public interface IDataRelay
    {
        /// <summary>
        ///     Create a user account. HTTP 409 if the username already exists. HTTP 500 if some other error.
        /// </summary>
        /// <param name="username">the username of the new account</param>
        /// <param name="password">the password of the new account</param>
        /// <param name="birthyear">the birthyear of the new account, can be null</param>
        /// <param name="weight">the weight in lbs of the new account, can be null</param>
        /// <param name="sex">"male" or "female". the sex of the new account, can be null</param>
        /// <param name="height">the height in inches of the new account, can be null</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Account/Create",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void CreateNewAccount(string username, string password, int? birthyear, int? weight, string sex, int? height);

        /// <summary>
        ///     Edit an account. Must be logged in. All info for account is replaced with the values supplied in this call. HTTP
        ///     401 if not logged in. HTTP 500 if some error.
        /// </summary>
        /// <param name="username">the new username of the account</param>
        /// <param name="password">the new password of the account</param>
        /// <param name="birthyear">the new birthyear of the account, can be null</param>
        /// <param name="weight">the new weight in lbs of the account, can be null</param>
        /// <param name="sex">"male" or "female". the new sex of the account, can be null</param>
        /// <param name="height">the new height in inches of the account, can be null</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Account/Edit",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void EditAccount(string username, string password, int? birthyear, int? weight, string sex, int? height);

        /// <summary>
        ///     Login an account with the credentials provided. Returns an <see cref="LoginToken" /> for future logged-in requests.
        ///     HTTP 401 if the credentials are incorrect. HTTP 500 if some other error.
        /// </summary>
        /// <param name="username">the username of the account to login</param>
        /// <param name="password">the password of the account to login</param>
        /// <returns>a <see cref="LoginToken" /> representing the session</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Login",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        LoginToken Login(string username, string password);

        /// <summary>
        ///     Gets the info for the account. Must be logged in. HTTP 401 if not logged in. HTTP 500 if some other error.
        /// </summary>
        /// <returns>an <see cref="Account" /> containing all the account's info</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Account",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Account GetAccountInfo();

        /// <summary>
        ///     Publishes and saves a finished activity. Must be logged in. HTTP 401 if not logged in. HTTP 500 if some other
        ///     error.
        /// </summary>
        /// <param name="time_started">date when the activity started recording, in the format of yyyy-MM-dd'T'HH:mm:ss</param>
        /// <param name="duration">time between the start and end of the activity recording, in the format of HH:mm:ss</param>
        /// <param name="mileage">miles traveled on this activity</param>
        /// <param name="calories_burned">calories burned on this activity</param>
        /// <param name="exercise_type">type of activity: "bike", "run", or "walk"</param>
        /// <param name="path">a space-separated list of coordinates, where each coordinate is a comma-separated lat and long</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Activity",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void CreateNewActivity(string time_started, string duration, float mileage, int calories_burned,
            string exercise_type, string path);

        /// <summary>
        ///     Gets the activity history belonging to an account. Must be logged in. HTTP 401 if not logged in. HTTP 500 if some
        ///     other error.
        /// </summary>
        /// <returns>all the activities associated with the account</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Activity",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Activity[] GetActivitiesForUser();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="gps"></param>
        /// <param name="imageLink"></param>
        /// <param name="date"></param>
        /// <param name="type"></param>
        /// <param name="color"></param>
        /// <param name="username"></param>
        /// <param name="active"></param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Ticket",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void CreateNewTicket(string title, string description, string gps, string imageLink, string date,
            string type, string color, string username, int active);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Ticket",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]

        Ticket[]  GetTickets();

        /// <summary>
        ///     Gets the total statistics for the account. Must be logged in. Broken up into 4: overall, biking, running, and
        ///     walking. HTTP 401 if not logged in. HTTP 500 if some other error.
        /// </summary>
        /// <returns>all the statistics associated with the account</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]

        TotalStat[] GetTotalStatsForUser();

        /// <summary>
        ///     Gets all statistics across every activity across every user. Broken up into 4: overall, biking, running, and
        ///     walking. HTTP 500 if some error.
        /// </summary>
        /// <returns>all the statistics from every activity</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/All",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        AllStat[] GetAllStats();

        /// <summary>
        ///     Gets all the paths across every activity across every user. HTTP 500 if some error.
        /// </summary>
        /// <returns>all the paths from every activity</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Path/All",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Path[] GetPath();
    }
}