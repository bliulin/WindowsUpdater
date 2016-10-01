using System.Configuration;

namespace Cegeka.Updater.Logic.Configuration
{
    public class LocalConfiguration : ILocalConfiguration
    {
        #region Properties

        public string CustomerName
        {
            get { return ConfigurationManager.AppSettings["CustomerName"]; }
        }

        public string GroupName
        {
            get { return ConfigurationManager.AppSettings["GroupName"]; }
        }

        public string ConfigurationFileUrl
        {
            get { return ConfigurationManager.AppSettings["ConfigurationFileUrl"]; }
        }

        public string DatabaseConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["StatusReportingServer"].ConnectionString; }
        }

        public string MonitoringServiceTemplateUrl
        {
            get { return ConfigurationManager.AppSettings["MonitoringServiceTemplateUrl"]; }
        }

        public string MonitoringServiceHost
        {
            get { return ConfigurationManager.AppSettings["MonitoringServiceHost"]; }
        }

        public int ReadConfigFileRetryCount
        {
            get
            {
                var value = ConfigurationManager.AppSettings["ReadConfigFileRetryCount"];
                if (string.IsNullOrEmpty(value))
                {
                    return 48;
                }
                return int.Parse(value);
            }
        }

        #endregion
    }
}