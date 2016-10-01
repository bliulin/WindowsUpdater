using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cegeka.Updater.Setup.CustomActions
{
    public interface ISessionWrapper
    {
        string Get(string propertyName);
        void Log(string msg);
        void Set(string propertyName, string value);
    }
}
