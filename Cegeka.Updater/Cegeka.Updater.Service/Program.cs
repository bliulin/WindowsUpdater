using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Cegeka.Updater.Logic;
using Cegeka.Updater.Logic.Utils;
using log4net;
using log4net.Config;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Quartz;
using Quartz.Impl;

namespace Cegeka.Updater.Service
{
    internal static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] arguments)
        {
            XmlConfigurator.Configure();                       

            if (arguments.Length > 0 && arguments[0].ToUpper().Replace("/", string.Empty).Replace("-", string.Empty) == "C")
            {
                Log.Info("Starting Cegeka Update Service Console host.");
                Console.WriteLine("Starting Cegeka Update Service Console host.");

                var updateController = InstanceFactory.GetInstance<IUpdateController>();
                updateController.Inactivated += updateController_InactiveStateActivated;

                updateController.BeginUpdate();

                Console.WriteLine("Press enter to stop the service.");
                Console.ReadLine();

                Console.WriteLine("Stopping Cegeka Update Service Console host.");

                updateController.Stop();
                Log.Info("Stopped.");
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                      {
                          new CegekaUpdateService()
                      };
                ServiceBase.Run(ServicesToRun);
            }
        }

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

        static void updateController_InactiveStateActivated(object sender, EventArgs e)
        {
            var timer = new System.Timers.Timer(WaitPeriod * 60000);
            timer.AutoReset = false;
            timer.Elapsed += (ts, te) => startUpdater();
            timer.Start();
        }

        private static void startUpdater()
        {
            var updateController = InstanceFactory.GetInstance<IUpdateController>();
            updateController.BeginUpdate();
        }        
    }
}