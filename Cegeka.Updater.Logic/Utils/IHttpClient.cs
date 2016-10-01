using System.Collections.Generic;

namespace Cegeka.Updater.Logic.Utils
{
    public interface IHttpClient
    {
        string GetResponse(string url);
        string GetResponse(string url, string hostHeader);
    }
}