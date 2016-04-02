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
            UriTemplate = "trails/api/1/Account/Create",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void CreateNewAccount(string username, string password, int dob, int weight, string sex, int height);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Account/{username}",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Account[] GetAccountInfo(string username);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "trails/api/1/Activity",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        int CreateNewActivity(string username, string time_started, string duration, float mileage, int calories_burned, string exercise_type, string path);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Activity/{username}",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Activity[] GetActivitiesForUser(string username);

        [OperationContract]
        [WebInvoke(Method = "GET",
             UriTemplate = "trails/api/1/Statistics/{username}",
             ResponseFormat = WebMessageFormat.Json,
             BodyStyle = WebMessageBodyStyle.Bare)]
        TotalStat[] GetTotalStatsForUser(string username);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Statistics/All",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        AllStat[] GetAllStats();

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "trails/api/1/Path/All",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        Path[] GetPath();
    }
}
