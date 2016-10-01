using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Microsoft.Practices.Unity;
using System.ServiceProcess;
using System.Threading;
using Cegeka.Updater.Logic;

namespace Cegeka.Updater.Service
{
    public partial class CegekaUpdateService : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static int WaitPeriod
        {
            get
            {
                var configValue = ConfigurationManager.AppSettings["TimeoutPeriodMinutes"];
                int waitPeriod;
                if (!int.TryParse(configValue, out waitPeriod))
                {
                    return 30;
                }
                return waitPeriod;
            }
        }

        public CegekaUpdateService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.Info("Starting Cegeka Update Service service.");

            try
            {
                var updateController = InstanceFactory.GetInstance<IUpdateController>();
                updateController.Inactivated += updateController_InactiveStateActivated;

                updateController.BeginUpdate();
            }
            catch (Exception ex)
            {
                Log.Error("Error starting service", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            Log.Info("Stopping Cegeka Update Service service.");

            try
            {
                var updateController = InstanceFactory.GetInstance<IUpdateController>();
                updateController.Stop();
            }
            catch (Exception ex)
            {
                Log.Error("Error stopping service", ex);
                throw;
            }
        }

        private void updateController_InactiveStateActivated(object sender, EventArgs e)
        {
            var timer = new System.Timers.Timer(WaitPeriod * 60000);
            timer.AutoReset = false;
            timer.Elapsed += (ts, te) => startUpdater();
            timer.Start();
        }

        private void startUpdater()
        {
            var updateController = InstanceFactory.GetInstance<IUpdateController>();
            updateController.BeginUpdate();
        }
    }
}