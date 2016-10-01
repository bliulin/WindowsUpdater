using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using log4net.Repository.Hierarchy;

namespace Cegeka.Updater.Logic.Utils
{
    class HttpClient : IHttpClient
    {
        public string GetResponse(string url)
        {
            var client = new WebClient();
            return client.DownloadString(url);
        }

        public string GetResponse(string url, string hostHeader)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.Headers.GetType().InvokeMember("ChangeInternal", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                null, request.Headers, new object[] { "Host", hostHeader });
            request.GetResponse();

            return null;
        }
    }
}
