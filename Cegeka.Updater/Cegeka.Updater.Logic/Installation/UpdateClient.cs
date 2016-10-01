using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Cegeka.Updater.Logic.Reporting;
using log4net;
using WUApiLib;

namespace Cegeka.Updater.Logic.Installation
{
    public class UpdateClient : IUpdateClient
    {
        #region Constructors

        public UpdateClient()
        {
            Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        #endregion

        #region >> IUpdateClient Members

        public IUpdateCollection GetAvailableUpdates()
        {
            var uSession = new UpdateSessionClass();
            IUpdateSearcher uSearcher = uSession.CreateUpdateSearcher();

            Logger.DebugFormat("Searcher server selection: " + uSearcher.ServerSelection);
            Logger.DebugFormat("Searcher service ID: " + uSearcher.ServiceID);

            uSearcher.Online = false;
            ISearchResult uResult = uSearcher.Search("IsInstalled=0 and Type='Software'");

            return uResult.Updates;
        }

        public Result InstallUpdates(IUpdateCollection updates)
        {
            //TODO: use NetBIOS name (until first .)

            Logger.Debug("START InstallUpdates");

            var updateLog = new UpdateInstallationLog();

            var uSession = new UpdateSessionClass();
            var downloader = uSession.CreateUpdateDownloader();
            downloader.Updates = (UpdateCollection) updates;

            Logger.Debug("Downloading the following updates:");
            foreach (IUpdate update in updates)
            {
                Logger.DebugFormat("  KB = '{0}', Title = '{1}'", update.KBArticleIDs[0], update.Title);
            }

            downloader.Download();

            var updatesToInstall = new UpdateCollectionClass();

            foreach (IUpdate update in updates)
            {
                if (update.IsDownloaded)
                {
                    bool addUpdate = true;

                    if (!update.EulaAccepted)
                    {
                        update.AcceptEula();
                    }

                    string message = string.Empty;

                    if (update.InstallationBehavior.CanRequestUserInput)
                    {
                        addUpdate = false;
                        message = Constants.CMessageUpdateCanRequestUserInput;
                        Logger.DebugFormat("KB='{0}', Title='{1}' can request user input and it will not be installed.", update.KBArticleIDs[0], update.Title);
                    }

                    if (addUpdate)
                    {
                        updatesToInstall.Add(update);                        
                    }
                    else
                    {
                        Logger.DebugFormat("KB='{0}', Title='{1}' was not installed because: {2}", update.KBArticleIDs[0], update.Title, message);
                        updateLog.Add(new UpdateInstallationLogEntry(update.KBArticleIDs[0], InstallationStatus.NotAttempted, message, DateTime.UtcNow));
                    }
                }
                else
                {
                    Logger.DebugFormat("KB='{0}', Title='{1}' was in the list of available updates but it was not downloaded.", update.KBArticleIDs[0], update.Title);
                    updateLog.Add(new UpdateInstallationLogEntry(update.KBArticleIDs[0], InstallationStatus.NotAttempted, Constants.CMessageUpdateNotDownloaded, DateTime.UtcNow));
                }
            }

            Logger.Debug("Installing the following updates:");
            foreach (IUpdate update in updatesToInstall)
            {
                var sb = new StringBuilder();
                foreach (var id in update.KBArticleIDs)
                {
                    sb.Append(id + ", ");
                }
                string kbArticleIds = sb.ToString();
                kbArticleIds = kbArticleIds.Remove(kbArticleIds.Length - 2);

                Logger.DebugFormat("  KB = '{0}', Title = '{1}', KB_Articles = '{2}'", update.KBArticleIDs[0], update.Title, kbArticleIds);
            }

            IInstallationResult installationResult;
            if (updatesToInstall.Count == 0)
            {
                Logger.Debug("No updates to install.");
                installationResult = new EmptyInstallationResult();
            }
            else
            {
                IUpdateInstaller installer = uSession.CreateUpdateInstaller();
                installer.Updates = updatesToInstall;
                
                installationResult = installer.Install();
                
                for (int i = 0; i < updatesToInstall.Count; i++)
                {
                    var result = installationResult.GetUpdateResult(i);
                    if (result.ResultCode == OperationResultCode.orcSucceeded)
                    {
                        updateLog.Add(new UpdateInstallationLogEntry(updatesToInstall[i].KBArticleIDs[0], InstallationStatus.Success, "", DateTime.UtcNow));
                    }
                    else
                    {
                        updateLog.Add(new UpdateInstallationLogEntry(updatesToInstall[i].KBArticleIDs[0], InstallationStatus.Failure
                            , "Operation result: " + result.ResultCode
                            , DateTime.UtcNow));
                    }
                }
            }

            Logger.Debug("END InstallUpdates");

            var processResult = new Result
            {
                InstallationResult = installationResult,
                UpdateInstallationLog = updateLog
            };

            return processResult;
        }

        #endregion

        #region Statics

        internal static ILog Logger { get; set; }

        #endregion
    }
}