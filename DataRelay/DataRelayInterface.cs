﻿using System.ServiceModel;
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
        /// <param name="id">ticket id number</param>
        /// <param name="type">type description of ticket. Ex: Water</param>
        /// <param name="description">description for what or where problem is</param>
        /// <param name="active">fixed or not</param>
        /// <param name="imgLink">file location</param>
        /// <param name="gps">gps coordinates for problem</param>
        /// <param name="title">name for the problem</param>
        /// <param name="date">date ticket was submitted</param>
        /// <param name="username">submitted by username</param>
        /// <param name="notes">additional notes about problem. Ex: fixed by someone</param>
        /// <param name="color">color associated with ticket type</param>
        /// <param name="dateClosed">date the ticket was closed</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Ticket/Create",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void CreateNewTicket(string type, string description, int? active, string imgLink, double latitude,
            double longitude, string title, string date, string username, string notes, string dateClosed);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id of ticket to close</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Ticket/Close",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void CloseTicket(int id);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>All the tickets in datebase</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Ticket/Active",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Ticket[] GetActiveTickets();

        [OperationContract]
        [WebInvoke(Method = "GET",
           UriTemplate = "trails/api/1/Ticket/Closed",
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare)]
        Ticket[] GetClosedTickets();

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

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/Accounts",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        int GetAccountsCount();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/Accounts/Gender",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        int[] GetGenderCount();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/Accounts/Age",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        int[] GetAgeCount();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/Accounts/Month",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        int[] GetMonthCount();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/Activities",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        int[] GetActivityStats();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/Activities/Time",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        int[] GetActivityTimeStats();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/Tickets",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        int[] GetTicketStats();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/Tickets/Compare",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        int[] CompareTicketStats();

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

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Retrieve/Image?id={id}",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        string getImageLink(int id);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Retrieve/Gps?id={id}",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        string getGPS(int id);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Retrieve/Note?id={id}",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        string getNote(int id);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Set/Note",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void setNotes(int id, string note);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Retrieve/Priority?id={id}",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        int getPriority(int id);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Set/Priority",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void setPriority(int id);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Retrieve/Title?id={id}",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        string getTicketTitle(int id);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Sort/MostRecent",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Ticket[] sortByMostRecent();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Sort/LeastRecent",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Ticket[] sortByLeastRecent();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Sort/Type",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Ticket[] sortByTicketType();

    }
}