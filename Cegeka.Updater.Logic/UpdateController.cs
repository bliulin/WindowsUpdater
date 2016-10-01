using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Cegeka.Updater.Logic.Configuration;
using Cegeka.Updater.Logic.Configuration.Model;
using Cegeka.Updater.Logic.Installation;
using Cegeka.Updater.Logic.Reporting;
using Cegeka.Updater.Logic.Schedule;
using Cegeka.Updater.Logic.Utils;
using log4net;
using WUApiLib;

namespace Cegeka.Updater.Logic
{
    public class UpdateController : IUpdateController
    {
        #region Events

        public event EventHandler<EventArgs> Inactivated;

        #endregion

        #region Instance fields

        private readonly List<string> mExcludedUpdates = new List<string>();

        private readonly ManualResetEvent mStopServiceEvent;

        private readonly object mSyncObject = new object();

        private CustomerConfiguration mCentralConfiguration;

        private string mCentralServerUrl;

        private string mCustomerName;

        private bool mInactivated;

        private string mMachineName;

        private uint mRetryCount;

        private string mServerGroupName;

        #endregion

        #region Properties

        public ILocalConfiguration LocalConfig { get; set; }

        public IHttpClient WebHttpClient { get; set; }

        public IUpdateClient WindowsUpdateClient { get; set; }

        public ITaskHandler WindowsTaskHandler { get; set; }

        public IStatusReporter Reporter { get; set; }

        public IReportStorage ReportStorage { get; set; }

        public ITimeFrameDecision TimeFrameDecision
        {
            get;
            set;
        }

        private string MachineName
        {
            get
            {
                if (mMachineName == null)
                {
                    mMachineName = WindowsTaskHandler.GetMachineName();
                }
                return mMachineName;
            }
        }

        public ILog Logger { get; set; }

        #endregion

        #region Constructors

        public UpdateController()
        {
            mStopServiceEvent = new ManualResetEvent(false);

            LocalConfig = InstanceFactory.GetInstance<ILocalConfiguration>();
            WebHttpClient = InstanceFactory.GetInstance<IHttpClient>();
            WindowsUpdateClient = InstanceFactory.GetInstance<IUpdateClient>();
            WindowsTaskHandler = InstanceFactory.GetInstance<ITaskHandler>();
            Reporter = InstanceFactory.GetInstance<IStatusReporter>();
            Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public UpdateController(ILocalConfiguration config, IHttpClient httpClient, IUpdateClient updateClient,
            ITaskHandler taskHandler, IStatusReporter reporter, IReportStorage reportStorage, ITimeFrameDecision timeFrameDecision)
        {
            mStopServiceEvent = new ManualResetEvent(false);

            LocalConfig = config;
            WebHttpClient = httpClient;
            WindowsUpdateClient = updateClient;
            WindowsTaskHandler = taskHandler;
            Reporter = reporter;
            ReportStorage = reportStorage;
            TimeFrameDecision = timeFrameDecision;

            Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        #endregion

        #region >> IUpdateController Members

        public void BeginUpdate()
        {
            ThreadPool.QueueUserWorkItem(obj => runUpdateProcedure());
        }

        public void Stop()
        {
            mStopServiceEvent.WaitOne();
        }

        #endregion

        #region Private Methods

        private void runUpdateProcedure()
        {
            lock (mSyncObject)
            {
                try
                {
                    Logger.Debug("START runUpdateProcedure");

                    mInactivated = false;

                    mStopServiceEvent.Reset();

                    logSystemInfo();

                    readLocalConfiguration();
                    readCentralConfiguration();
                    getExcludedUpdates();

                    if (mCentralConfiguration == null)
                    {
                        Logger.DebugFormat("Going idle - Customer name '{0}' was not found in the CustomerConfigurations section.", LocalConfig.CustomerName);
                        inactivate();
                        return;
                    }

                    if (isServerInExclusionList())
                    {
                        Logger.Debug("Going idle - Server is present in exclusion list.");
                        inactivate();
                        return;
                    }

                    if (!isWithinMaintenanceTimeFrame())
                    {
                        Logger.Debug("Going idle - Not within maintenance timeframe.");
                        inactivate();
                        return;
                    }

                    trySendSavedReports();

                    Logger.Debug("Fetching available updates.");
                    IUpdateCollection updates = WindowsUpdateClient.GetAvailableUpdates();

                    if (updates == null || updates.Count == 0)
                    {
                        Logger.Debug("Going idle - No updates retrieved from WUA.");
                        inactivate();
                        return;
                    }

                    var updatesToInstall = new UpdateCollectionClass();
                    var excludedUpdates = new UpdateCollectionClass();

                    for (int i = 0; i < updates.Count; i++)
                    {
                        IUpdate update = updates[i];
                        if (!isUpdateExcluded(update))
                        {
                            updatesToInstall.Add(update);
                        }
                        else
                        {
                            excludedUpdates.Add(update);
                        }
                    }

                    if (updatesToInstall.Count == 0)
                    {
                        if (excludedUpdates.Count > 0)
                        {
                            var entries = UpdateInstallationLog.CreateExcludedLogEntries(excludedUpdates);
                            var log = UpdateInstallationLog.Create(entries);
                            reportUpdateInstallationStatus(log, true);
                        }
                        Logger.Debug("Going idle - No new updates found.");
                        inactivate();
                        return;
                    }

                    Logger.Debug("Writing event log and monitoring API call.");
                    WindowsTaskHandler.WriteEventLog(Constants.CEventLogId, Constants.CEventLogDescription);
                    WindowsTaskHandler.CallMonitoringApi();

                    Logger.Debug("Installing updates.");
                    Result result = WindowsUpdateClient.InstallUpdates(updatesToInstall);

                    // add excluded updates to result
                    var excludedEntries = UpdateInstallationLog.CreateExcludedLogEntries(excludedUpdates);
                    result.UpdateInstallationLog.AddRange(excludedEntries);

                    reportStatus(result);

                    if (result.InstallationResult.RebootRequired)
                    {
                        Logger.Debug("Rebooting system.");
                        WindowsTaskHandler.RebootSystem();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                finally
                {
                    if (!mInactivated)
                    {
                        inactivate();
                    }

                    Logger.Debug("FINISH runUpdateProcedure");
                    mStopServiceEvent.Set();
                }
            }
        }

        private void trySendSavedReports()
        {
            Logger.Debug("Checking for saved reports...");

            var savedUpdateLog = loadReport();
            if (savedUpdateLog != null && savedUpdateLog.Count > 0)
            {
                Logger.Debug("Found saved reports, saving to database...");
                reportUpdateInstallationStatus(savedUpdateLog, false);
            }
            else
            {
                Logger.Debug("No saved reports found.");
            }
        }

        private void reportStatus(Result result)
        {
            Logger.Debug("START Installation report");
            Logger.Debug("Result code: " + result.InstallationResult.ResultCode);
            Logger.Debug("Reboot required: " + result.InstallationResult.RebootRequired);            

            Logger.Debug("Update log:");

            var sortedUpdateLog = result.UpdateInstallationLog.OrderBy(log => log.UpdateInstallationStatus);
            reportUpdateInstallationStatus(UpdateInstallationLog.Create(sortedUpdateLog), true);
            
            Logger.Debug("END Installation report");
        }

        private void reportUpdateInstallationStatus(UpdateInstallationLog updateLog, bool isCurrent)
        {
            Logger.Debug("Update log:");

            var sortedUpdateLog = UpdateInstallationLog.Create(updateLog.OrderBy(log => log.UpdateInstallationStatus));
            foreach (var updateInstallationLogEntry in sortedUpdateLog)
            {
                log(updateInstallationLogEntry);
                Reporter.ReportStatus(updateInstallationLogEntry);
            }

            bool success = commitWithRetry();
            if (!success)
            {
                Logger.Debug("Writing event log: " + Constants.CEventLogFailedToWriteToDatabase);
                WindowsTaskHandler.WriteEventLog(Constants.CEventLogId, EventLogEntryType.Warning, Constants.CEventLogFailedToWriteToDatabase);
            }

            if (isCurrent)
            {                
                if (!success)
                {
                    saveReport(sortedUpdateLog);
                }                
            }
            else
            {
                if (success)
                {
                    deleteReport();
                }                
            }

            Logger.Debug("Finished reporting installation status.");
        }

        private void deleteReport()
        {
            try
            {
                ReportStorage.DeleteReports();
                Logger.Debug("Deleted reports from disk.");
            }
            catch (Exception ex)
            {                
                Logger.Error("Failed to delete report file.", ex);
            }
        }

        private UpdateInstallationLog loadReport()
        {
            try
            {
                Logger.Debug("Loading report...");
                return ReportStorage.LoadReportLog();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load report.", ex);
            }

            return null;
        }

        private void saveReport(UpdateInstallationLog log)
        {
            try
            {
                Logger.Debug("Saving report to disk...");
                string path = ReportStorage.SaveReportLog(log);
                Logger.Debug("Report log was saved to disk: " + path);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to write report to disk: ", ex);
            }
        }

        private bool commitWithRetry()
        {
            var scheduler = InstanceFactory.GetInstance<IScheduler>();
            return Reporter.CommitWithRetry(scheduler);
        }

        private bool isUpdateExcluded(IUpdate update)
        {
            foreach (string kbid in update.KBArticleIDs)
            {
                if (mExcludedUpdates.Contains(kbid))
                {
                    Logger.DebugFormat("Update '{0}' is excluded and it will not be installed.", kbid);
                    return true;
                }
            }

            return false;
        }

        private void readCentralConfiguration()
        {
            mRetryCount++;

            string xmlData = null;

            Logger.Debug("Reading central configuration.");

            try
            {
                xmlData = WebHttpClient.GetResponse(LocalConfig.ConfigurationFileUrl);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to read central configuration " + LocalConfig.ConfigurationFileUrl, ex);
                WindowsTaskHandler.WriteEventLog(Constants.CEventLogId, EventLogEntryType.Warning, Constants.CEventLogFailedToRetrieveCentralConfiguration);
            }

            if (!string.IsNullOrEmpty(xmlData))
            {
                mRetryCount = 0;
                var serializer = new Serialization<CustomerConfigurationsFile>();
                mCentralConfiguration = serializer.GetObjectFromXml(xmlData).FirstOrDefault(cc => cc.CustomerName.Equals(mCustomerName, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                if (mRetryCount == LocalConfig.ReadConfigFileRetryCount)
                {
                    mCentralConfiguration = null;
                    mRetryCount = 0;
                }

                if (mCentralConfiguration == null)
                {
                    throw new Exception("Could not read central configuration file.");
                }
            }
        }

        private void readLocalConfiguration()
        {
            mCustomerName = LocalConfig.CustomerName;
            if (string.IsNullOrEmpty(mCustomerName))
            {
                throw new ConfigurationErrorsException("CustomerName is not configured.");
            }

            mServerGroupName = LocalConfig.GroupName;
            if (string.IsNullOrEmpty(mServerGroupName))
            {
                throw new ConfigurationErrorsException("GroupName is not configured.");
            }

            mCentralServerUrl = LocalConfig.ConfigurationFileUrl;
            if (string.IsNullOrEmpty(mCentralServerUrl))
            {
                throw new ConfigurationErrorsException("ServerUrl is not configured.");
            }
        }

        private bool isServerInExclusionList()
        {
            Logger.Debug("Testing if server is present in the exclusion list.");

            var sb = new StringBuilder();
            foreach (string excludedServer in mCentralConfiguration.ServerUpdateConfiguration.ExcludedServers)
            {
                sb.Append(excludedServer + " ");
            }
            Logger.Debug("Excluded servers: " + sb);

            bool serverIsExcluded = mCentralConfiguration.ServerUpdateConfiguration.ExcludedServers.Contains(MachineName);
            Logger.Debug("Server is present in exclusion list: " + serverIsExcluded);

            return serverIsExcluded;
        }

        private bool isWithinMaintenanceTimeFrame()
        {
            TimeFrame timeFrame =
                mCentralConfiguration.MaintenanceSchedule.FirstOrDefault(tf => 
                    tf.GroupName.Equals(LocalConfig.GroupName, StringComparison.InvariantCultureIgnoreCase));

            bool result = TimeFrameDecision.IsWithinTimeFrame(timeFrame);
            return result;
        }

        private void inactivate()
        {
            mInactivated = true;
            mStopServiceEvent.Set();

            EventHandler<EventArgs> handler = Inactivated;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void log(UpdateInstallationLogEntry logEntry)
        {
            var sb = new StringBuilder();
            sb.AppendLine("KB ID: " + logEntry.KbArticleId);
            sb.AppendLine("Status: " + logEntry.UpdateInstallationStatus);
            sb.AppendLine("Time: " + logEntry.ProcessedTime);
            if (!string.IsNullOrEmpty(logEntry.Message))
            {
                sb.AppendLine("Message: " + logEntry.Message);
            }
            Logger.Debug(sb);
        }

        private void logSystemInfo()
        {
            Logger.Debug("Machine name: " + MachineName);
            Logger.Debug("Operating system: " + Environment.OSVersion);
            Logger.Debug("CLR version: " + Environment.Version);
        }

        private void getExcludedUpdates()
        {
            ServerConfigurationItem serverConfig = mCentralConfiguration.ServerUpdateConfiguration.Servers
                .FirstOrDefault(s => s.Name.Equals(MachineName, StringComparison.InvariantCultureIgnoreCase));
            if (serverConfig == null)
            {
                Logger.Debug("There are no excluded updates configured for this server.");
                return;
            }

            Logger.DebugFormat("Excluded updates (KB IDs) for '{0}':", MachineName);
            var sb = new StringBuilder();
            foreach (string excludedUpdateKbArticle in serverConfig.ExcludedUpdateKbArticles)
            {
                sb.Append(excludedUpdateKbArticle + " ");
                mExcludedUpdates.Add(excludedUpdateKbArticle);
            }
            Logger.Debug(sb);
        }

        #endregion
    }
}