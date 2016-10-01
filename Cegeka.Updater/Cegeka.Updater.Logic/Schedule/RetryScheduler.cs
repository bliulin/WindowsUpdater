using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;
using log4net.Repository.Hierarchy;

namespace Cegeka.Updater.Logic.Schedule
{
    public class RetryScheduler : IScheduler
    {
        private ILog mLogger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ScheduleSettings mSettings;

        public ScheduleSettings Settings {
            get
            {
                if (mSettings == null)
                {
                    mSettings = new ScheduleSettings();
                    mSettings.RetryCount = Constants.CRetryCount;
                    mSettings.Timeout = Constants.CRetryTimeout;
                }
                return mSettings;
            }
            set
            {
                mSettings = value;
            }
        }

        public bool Execute(Func<bool> action)
        {
            mLogger.DebugFormat("START action '{0}' with retry.", action.Method.Name);

            bool ok = false;
            int retryCount = 1;

            do
            {
                try
                {
                    mLogger.Debug("Attempt #" + retryCount);
                    ok = action.Invoke();                    
                    mLogger.Debug("Execution was successful.");
                }
                catch (Exception ex)
                {
                    mLogger.Error(ex);
                    retryCount++;

                    mLogger.DebugFormat("Waiting {0} seconds between retries...", Settings.Timeout);
                    Thread.Sleep(Settings.Timeout * 1000);
                }                
            } while (!ok && retryCount <= Settings.RetryCount);

            mLogger.DebugFormat("END action '{0}' with retry: success = {1} after {2} attempts.", action.Method.Name, ok, (retryCount - 1));

            return ok;
        }
    }
}
