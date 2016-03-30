using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DataRelay
{
    [ServiceContract]
    public interface IDataRelay
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "/Account/Create",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        int CreateNewAccount(string username, string password, int dob, int weight, string sex, int height);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "/Account/{username}",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        Account[] GetAccountInfo(string username);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "/Activity",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        int CreateNewActivity(string username, string time_started, string duration, float mileage, int calories_burned, string exercise_type, string path);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "/Activity/{username}",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        Activity[] GetActivitiesForUser(string username);

        [OperationContract]
        [WebInvoke(Method = "GET",
             UriTemplate = "/Statistics/{username}",
             ResponseFormat = WebMessageFormat.Json,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        TotalStat[] GetTotalStatsForUser(string username);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "/AllStat",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        AllStat[] GetAllStats();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "/OverAllPath",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        Path[] GetPath();
    }
}
