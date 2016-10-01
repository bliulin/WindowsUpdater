using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Cegeka.Updater.Logic.Configuration;
using log4net;

namespace Cegeka.Updater.Logic.Utils
{
    internal class TaskHandler : ITaskHandler
    {
        #region Instance fields

        private readonly ILog mLogger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Properties

        public IHttpClient Client { get; set; }

        public ILocalConfiguration Configuration { get; set; }

        #endregion

        #region Constructors

        public TaskHandler(ILocalConfiguration configuration, IHttpClient httpClient)
        {
            Configuration = configuration;
            Client = httpClient;
        }

        #endregion

        #region >> ITaskHandler Members

        public void WriteEventLog(int eventLogId, string description)
        {
            try
            {
                WriteEventLog(eventLogId, EventLogEntryType.Information, description);
            }
            catch (Exception ex)
            {
                mLogger.Debug(ex);
            }
        }

        public void WriteEventLog(int eventLogId, EventLogEntryType eventLogEntryType, string description)
        {
            try
            {
                EventLog.WriteEntry(Constants.EventSourceName, description, eventLogEntryType, eventLogId);
            }
            catch (Exception ex)
            {
                mLogger.Debug(ex);
            }
        }

        public void CallMonitoringApi()
        {
            try
            {
                var url = string.Format(Configuration.MonitoringServiceTemplateUrl, GetMachineName());
                Client.GetResponse(url, Configuration.MonitoringServiceHost);
            }
            catch (Exception ex)
            {
                mLogger.Debug(ex);
            }
        }

        public string GetMachineName()
        {
            return Environment.MachineName;
        }

        public void RebootSystem()
        {
            Process.Start("shutdown", "/r /f /t 0");
        }

        #endregion
    }
}