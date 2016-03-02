using System;
using System.Collections.Generic;
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
        string[] GetAccountInfo(string username);
    }
}
