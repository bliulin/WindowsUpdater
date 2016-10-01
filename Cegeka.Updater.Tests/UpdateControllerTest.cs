using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cegeka.Updater.Logic;
using Cegeka.Updater.Logic.Configuration;
using Cegeka.Updater.Logic.Configuration.Model;
using Cegeka.Updater.Logic.Installation;
using Cegeka.Updater.Logic.Reporting;
using Cegeka.Updater.Logic.Schedule;
using Cegeka.Updater.Logic.Utils;
using Cegeka.Updater.Tests.Fakes;
using log4net;
using log4net.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WUApiLib;
using System.Linq;
namespace Cegeka.Updater.Tests
{
    [TestClass]
    public partial class UpdateControllerTest
    {
        private const string CServerName = "cegeka-api";
        private const string CGroupName = "Development";
        private const string CCustomerName = "Cegeka";
        private const string CServerUrl = "http://www.example.com";

        private UpdateController mController;

        #region Private methods
        private ILocalConfiguration getLocalConfig()
        {
            var localConfig = Mock.Of<ILocalConfiguration>();

            Mock.Get(localConfig).Setup(c => c.CustomerName).Returns(CCustomerName);
            Mock.Get(localConfig).Setup(c => c.GroupName).Returns(CGroupName);
            Mock.Get(localConfig).Setup(c => c.ConfigurationFileUrl).Returns(CServerUrl);

            return localConfig;
        }

        private IHttpClient getHttpClient()
        {
            string configContent = File.ReadAllText(@"..\..\centralConfig.xml");
            var serializer = new Serialization<CustomerConfigurationsFile>();
            var configInstance = serializer.GetObjectFromXml(configContent);
            var timeFrame = configInstance.FirstOrDefault().MaintenanceSchedule.FirstOrDefault(tf => tf.GroupName == CGroupName);
            if (timeFrame != null)
            {
                var ci = new CultureInfo("nl-NL");
                timeFrame.DayOfWeek = (int)DateTime.UtcNow.DayOfWeek;
                timeFrame.StartTime = DateTime.UtcNow.AddHours(-1).ToString(ci);
                timeFrame.EndTime = DateTime.UtcNow.AddHours(1).ToString(ci);
                timeFrame.WeekCount = (int)Math.Ceiling((decimal)DateTime.UtcNow.Day / 7);
            }
            else
            {
                throw new Exception("Time frame does not contain the test group.");
            }
            configContent = serializer.Serialize(configInstance);
            var httpClient = Mock.Of<IHttpClient>();
            Mock.Get(httpClient).Setup(c => c.GetResponse(It.IsAny<string>())).Returns(configContent);
            return httpClient;
        }

        private IHttpClient getHttpClientWithNoExcludedUpdates()
        {
            string configContent = File.ReadAllText(@"..\..\centralConfig.xml");
            var serializer = new Serialization<CustomerConfigurationsFile>();
            var configInstance = serializer.GetObjectFromXml(configContent);
            var timeFrame = configInstance.FirstOrDefault().MaintenanceSchedule.FirstOrDefault(tf => tf.GroupName == CGroupName);
            if (timeFrame != null)
            {
                var ci = new CultureInfo("nl-NL");
                timeFrame.DayOfWeek = (int)DateTime.UtcNow.DayOfWeek;
                timeFrame.StartTime = DateTime.UtcNow.AddHours(-1).ToString(ci);
                timeFrame.EndTime = DateTime.UtcNow.AddHours(1).ToString(ci);
                timeFrame.WeekCount = (int)Math.Ceiling((decimal)DateTime.UtcNow.Day / 7);
            }
            else
            {
                throw new Exception("Time frame does not contain the test group.");
            }

            var serverconfig = configInstance.FirstOrDefault().ServerUpdateConfiguration.Servers.FirstOrDefault(c => c.Name == CServerName);
            serverconfig.ExcludedUpdateKbArticles = new string[] { };

            configContent = serializer.Serialize(configInstance);
            var httpClient = Mock.Of<IHttpClient>();
            Mock.Get(httpClient).Setup(c => c.GetResponse(It.IsAny<string>())).Returns(configContent);
            return httpClient;
        }

        private UpdateController createController(ILocalConfiguration localConfig = null, IHttpClient httpClient = null, IUpdateClient updateClient = null,
          ILog logger = null, ITaskHandler taskHandler = null, IStatusReporter reporter = null, IReportStorage reportStorage = null, ITimeFrameDecision timeFrameDecision = null)
        {
            if (localConfig == null)
            {
                localConfig = getLocalConfig();
            }

            if (httpClient == null)
            {
                httpClient = getHttpClient();
            }

            if (updateClient == null)
            {
                updateClient = Mock.Of<IUpdateClient>();
            }

            if (logger == null)
            {
                logger = Mock.Of<ILog>();
            }

            if (taskHandler == null)
            {
                taskHandler = Mock.Of<ITaskHandler>();
                Mock.Get(taskHandler).Setup(t => t.GetMachineName()).Returns(CServerName);
            }

            if (reporter == null)
            {
                reporter = Mock.Of<IStatusReporter>();
            }

            if (reportStorage == null)
            {
                reportStorage = Mock.Of<IReportStorage>();
            }

            if (timeFrameDecision == null)
            {
                timeFrameDecision = Mock.Of<ITimeFrameDecision>();
                Mock.Get(timeFrameDecision).Setup(t => t.IsWithinTimeFrame(It.IsAny<TimeFrame>())).Returns(true);
            }

            var controller = new UpdateController(localConfig, httpClient, updateClient, taskHandler, reporter, reportStorage, timeFrameDecision);
            controller.Logger = Mock.Of<ILog>();

            return controller;
        }

        private UpdateController createControllerWithUpdatesToInstall(IStatusReporter reporter)
        {
            var updateClient = getUpdateClientWithUpdatesToInstall(new[] { "4269002", "2267602" });
            var controller = createController(null, null, updateClient, null, null, reporter);
            return controller;
        }

        private IUpdateClient getUpdateClientWithUpdatesToInstall(IEnumerable<string> kbIds)
        {
            var updateCollection = new UpdateCollectionClass();
            foreach (var kbId in kbIds)
            {
                IUpdate update = Mock.Of<IUpdate>();
                var kbIdsCollection = new StringCollectionClass();
                kbIdsCollection.Add(kbId);
                Mock.Get(update).Setup(u => u.KBArticleIDs).Returns(kbIdsCollection);
                updateCollection.Add(update);
            }

            var updateClient = Mock.Of<IUpdateClient>();
            Mock.Get(updateClient).Setup(uc => uc.GetAvailableUpdates()).Returns(updateCollection);

            var updateInstallationLogEntries = new UpdateInstallationLog();
            kbIds.ToList().ForEach(kbid => updateInstallationLogEntries.Add(new UpdateInstallationLogEntry(kbid, InstallationStatus.Success, "", DateTime.UtcNow)));

            var result = new Result
            {
                InstallationResult = Mock.Of<IInstallationResult>(),
                UpdateInstallationLog = updateInstallationLogEntries
            };

            Mock.Get(updateClient).Setup(uc => uc.InstallUpdates(It.IsAny<UpdateCollection>())).Returns(result);

            return updateClient;
        }

        #endregion

        [TestInitialize]
        public void Init()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
            mController = createController();
            registerTypes();
        }

        [TestMethod]
        public void Given_FailReadingCentralConfigAndCentralConfigNotInitialized_Then_InactivatedIsRaised()
        {
            var httpClient = Mock.Of<IHttpClient>();
            Mock.Get(httpClient).Setup(c => c.GetResponse(It.IsAny<string>())).Throws<Exception>();

            IUpdateClient updateClient = Mock.Of<IUpdateClient>();
            mController = createController(null, httpClient, updateClient);
            var run = mController.GetType().GetMethod("runUpdateProcedure", BindingFlags.NonPublic | BindingFlags.Instance);

            bool inactive = false;

            mController.Inactivated += (s, e) => { inactive = true; };
            run.Invoke(mController, new object[] { });

            while (!inactive)
            {
                Thread.Sleep(0);
            }

            Mock.Get(updateClient).Verify(c => c.GetAvailableUpdates(), Times.Never);
        }               

        [TestMethod]
        public void Given_MaintenanceTimeFrame_When_IsNotWithinMaintenanceHours_Then_InactivatedIsRaised()
        {
            var cultureInfo = new CultureInfo("nl-NL");

            string configContent = File.ReadAllText(@"..\..\centralConfig.xml");
            var serializer = new Serialization<CustomerConfigurationsFile>();
            var configInstance = serializer.GetObjectFromXml(configContent);

            var timeFrame = configInstance.FirstOrDefault().MaintenanceSchedule.FirstOrDefault(tf => tf.GroupName == CGroupName);
            timeFrame.DayOfWeek = (int)DateTime.UtcNow.DayOfWeek;
            timeFrame.StartTime = DateTime.UtcNow.AddMinutes(10).ToString(cultureInfo);
            timeFrame.EndTime = DateTime.UtcNow.AddMinutes(30).ToString(cultureInfo);

            configContent = serializer.Serialize(configInstance);

            var httpClient = Mock.Of<IHttpClient>();
            Mock.Get(httpClient).Setup(c => c.GetResponse(It.IsAny<string>())).Returns(configContent);

            var timeFrameDecision = Mock.Of<ITimeFrameDecision>();
            Mock.Get(timeFrameDecision).Setup(t => t.IsWithinTimeFrame(It.IsAny<TimeFrame>())).Returns(false);

            IUpdateClient updateClient = Mock.Of<IUpdateClient>();
            mController = createController(null, httpClient, updateClient, timeFrameDecision: timeFrameDecision);
            var run = mController.GetType().GetMethod("runUpdateProcedure", BindingFlags.NonPublic | BindingFlags.Instance);

            bool inactive = false;

            mController.Inactivated += (s, e) => { inactive = true; };
            run.Invoke(mController, new object[] { });

            while (!inactive)
            {
                Thread.Sleep(0);
            }

            Mock.Get(updateClient).Verify(c => c.GetAvailableUpdates(), Times.Never);
        }

        [TestMethod]
        public void Given_TimeFrameMaintenanceThrowsException_Then_InactivatedIsRaised()
        {
            var cultureInfo = new CultureInfo("nl-NL");

            string configContent = File.ReadAllText(@"..\..\centralConfig.xml");
            var serializer = new Serialization<CustomerConfigurationsFile>();
            var configInstance = serializer.GetObjectFromXml(configContent);

            var timeFrame = configInstance.FirstOrDefault().MaintenanceSchedule.FirstOrDefault(tf => tf.GroupName == CGroupName);
            timeFrame.DayOfWeek = (int)DateTime.UtcNow.DayOfWeek;
            timeFrame.StartTime = DateTime.UtcNow.AddMinutes(10).ToString(cultureInfo);
            timeFrame.EndTime = DateTime.UtcNow.AddMinutes(30).ToString(cultureInfo);

            configContent = serializer.Serialize(configInstance);

            var httpClient = Mock.Of<IHttpClient>();
            Mock.Get(httpClient).Setup(c => c.GetResponse(It.IsAny<string>())).Returns(configContent);

            var timeFrameDecision = Mock.Of<ITimeFrameDecision>();
            Mock.Get(timeFrameDecision).Setup(t => t.IsWithinTimeFrame(It.IsAny<TimeFrame>())).Throws(new ConfigurationErrorsException());

            IUpdateClient updateClient = Mock.Of<IUpdateClient>();
            mController = createController(null, httpClient, updateClient, timeFrameDecision: timeFrameDecision);
            var run = mController.GetType().GetMethod("runUpdateProcedure", BindingFlags.NonPublic | BindingFlags.Instance);

            bool inactive = false;

            mController.Inactivated += (s, e) => { inactive = true; };
            run.Invoke(mController, new object[] { });

            while (!inactive)
            {
                Thread.Sleep(0);
            }

            Mock.Get(updateClient).Verify(c => c.GetAvailableUpdates(), Times.Never);
        }

        [TestMethod]
        public void Given_MaintenanceTimeFrame_When_IsWithinMaintenanceHours_Then_GetAvailableUpdatesIsCalled()
        {
            var cultureInfo = new CultureInfo("nl-NL");

            string configContent = File.ReadAllText(@"..\..\centralConfig.xml");
            var serializer = new Serialization<CustomerConfigurationsFile>();
            var configInstance = serializer.GetObjectFromXml(configContent);

            configContent = serializer.Serialize(configInstance);

            var httpClient = Mock.Of<IHttpClient>();
            Mock.Get(httpClient).Setup(c => c.GetResponse(It.IsAny<string>())).Returns(configContent);

            IUpdateClient updateClient = Mock.Of<IUpdateClient>();
            mController = createController(null, httpClient, updateClient);
            var run = mController.GetType().GetMethod("runUpdateProcedure", BindingFlags.NonPublic | BindingFlags.Instance);

            bool inactive = false;

            mController.Inactivated += (s, e) => { inactive = true; };
            run.Invoke(mController, new object[] { });

            while (!inactive)
            {
                Thread.Sleep(0);
            }

            Mock.Get(updateClient).Verify(c => c.GetAvailableUpdates(), Times.Once);
        }

        [TestMethod]
        public void Given_ServerIsPresentInExclusionList_Then_InactiveStateActivated()
        {
            string configContent = File.ReadAllText(@"..\..\centralConfig.xml");
            var serializer = new Serialization<CustomerConfigurationsFile>();
            var configInstance = serializer.GetObjectFromXml(configContent);
            configInstance.FirstOrDefault().ServerUpdateConfiguration.ExcludedServers[0] = CServerName;
            configContent = serializer.Serialize(configInstance);

            var httpClient = Mock.Of<IHttpClient>();
            Mock.Get(httpClient).Setup(c => c.GetResponse(It.IsAny<string>())).Returns(configContent);

            mController = createController(null, httpClient);

            bool inactive = false;
            mController.Inactivated += (s, e) => { inactive = true; };
            mController.BeginUpdate();

            while (!inactive) Thread.Sleep(0);

            Assert.IsTrue(inactive);
        }

        [TestMethod]
        public void Given_GroupIsNotInMaintenanceTimeFrame_Then_InactiveStateActivated()
        {
            string configContent = File.ReadAllText(@"..\..\centralConfig.xml");
            var serializer = new Serialization<CustomerConfigurationsFile>();
            var configInstance = serializer.GetObjectFromXml(configContent);
            var timeFrame = configInstance.FirstOrDefault().MaintenanceSchedule.FirstOrDefault(tf => tf.GroupName == CGroupName);
            if (timeFrame != null)
            {
                timeFrame.StartTime = DateTime.UtcNow.AddHours(1).ToString();
                timeFrame.EndTime = DateTime.UtcNow.AddHours(2).ToString();
            }
            else
            {
                throw new Exception("Time frame does not contain the test group.");
            }

            if (configInstance.FirstOrDefault().ServerUpdateConfiguration.ExcludedServers.Contains(CServerName))
            {
                throw new Exception("Server is in the exclusion list.");
            }

            configContent = serializer.Serialize(configInstance);
            var httpClient = Mock.Of<IHttpClient>();
            Mock.Get(httpClient).Setup(c => c.GetResponse(It.IsAny<string>())).Returns(configContent);

            mController = createController(null, httpClient);

            bool inactive = false;
            mController.Inactivated += (s, e) => { inactive = true; };
            mController.BeginUpdate();

            while (!inactive) Thread.Sleep(0);

            Assert.IsTrue(inactive);
        }

        [TestMethod]
        public void Given_ServerIsNotPresentInExclusionListAndGroupIsInMaintenanceTimeFrame_Then_GetUpdatesIsCalled()
        {
            var updateClient = Mock.Of<IUpdateClient>();
            mController = createController(null, getHttpClient(), updateClient);

            bool inactive = false;
            mController.Inactivated += (s, e) => { inactive = true; };
            mController.BeginUpdate();

            while (!inactive) Thread.Sleep(0);

            Mock.Get(updateClient).Verify(uc => uc.GetAvailableUpdates(), Times.Once);
        }

        [TestMethod]
        public void Given_GetAvailableUpdatesReturnsNull_Then_InactiveStateActivated()
        {
            var updateClient = Mock.Of<IUpdateClient>();
            Mock.Get(updateClient).Setup(c => c.GetAvailableUpdates()).Returns((IUpdateCollection)null);

            var httpClient = getHttpClient();
            mController = createController(null, httpClient, updateClient);

            bool inactive = false;
            mController.Inactivated += (s, e) => { inactive = true; };
            mController.BeginUpdate();

            while (!inactive) Thread.Sleep(0);

            Assert.IsTrue(inactive);
        }

        [TestMethod]
        public void Given_GetAvailableUpdatesReturnsEmpty_Then_InactiveStateActivated()
        {
            var updateClient = Mock.Of<IUpdateClient>();
            Mock.Get(updateClient).Setup(c => c.GetAvailableUpdates()).Returns(new UpdateCollectionClass());

            var httpClient = getHttpClient();
            mController = createController(null, httpClient, updateClient);

            bool inactive = false;
            mController.Inactivated += (s, e) => { inactive = true; };
            mController.BeginUpdate();

            while (!inactive)
            {
                Thread.Sleep(0);
            }

            Assert.IsTrue(inactive);
        }

        [TestMethod]
        public void Given_TwoUpdatesAvailable_When_UpdatesPresentInExclusionList_Then_InactiveStateActivated()
        {            
            var updateClient = getUpdateClientWithUpdatesToInstall(new[] { "4269002", "2267602" });
            mController = createController(null, null, updateClient);
            bool inactive = false;
            mController.Inactivated += (s, e) => { inactive = true; };
            mController.BeginUpdate();

            while (!inactive)
            {
                Thread.Sleep(0);
            }

            Assert.IsTrue(inactive);
        }

        [TestMethod]
        public void Given_UpdatesReadyForInstall_Then_WriteEventLogIsCalled()
        {
            //IUpdate update1 = Mock.Of<IUpdate>();
            //IUpdate update2 = Mock.Of<IUpdate>();

            //string updateId1 = "230b82d1-3abd-471a-a4f9-23f97fb857d9";
            //string updateId2 = "8497fe19-9f28-4189-b671-b95d8ba8c2d9";

            //Mock.Get(update1).Setup(u => u.Identity.UpdateID).Returns(updateId1);
            //Mock.Get(update2).Setup(u => u.Identity.UpdateID).Returns(updateId2);

            //var updateCollection = new UpdateCollectionClass();
            //updateCollection.Add(update1);
            //updateCollection.Add(update2);

            //var updateClient = Mock.Of<IUpdateClient>();
            //Mock.Get(updateClient).Setup(u => u.GetAvailableUpdates()).Returns(updateCollection);
            //Mock.Get(updateClient).Setup(u => u.InstallUpdates(updateCollection)).Returns(new Result()
            //{
            //    ProcessedUpdates = new UpdateCollectionClass(),
            //    InstallationResult = Mock.Of<IInstallationResult>(),
            //    NotProcessedUpdates = new UpdateCollectionClass(),
            //    Reasons = new string[] { }
            //});

            //var taskHandler = Mock.Of<ITaskHandler>();

            //var httpClient = getHttpClientWithNoExcludedUpdates();
            //mController = createController(null, httpClient, updateClient, null, taskHandler);

            //mController.BeginUpdate();

            //bool inactive = false;
            //mController.InactiveStateActivated += (s, e) => { inactive = true; };

            //while (!inactive) Thread.Sleep(0);

            //Mock.Get(taskHandler).Verify(h => h.WriteEventLog(187, "Cegeka automatic patch management"), Times.Once);
        }

        [TestMethod]
        public void Given_UpdatesInstalled_When_IsRebootNeeded_Then_RebootSystemIsCalled()
        {
            //IUpdate update1 = Mock.Of<IUpdate>();
            //IUpdate update2 = Mock.Of<IUpdate>();

            //string updateId1 = "230b82d1-3abd-471a-a4f9-23f97fb857d9";
            //string updateId2 = "8497fe19-9f28-4189-b671-b95d8ba8c2d9";

            //Mock.Get(update1).Setup(u => u.Identity.UpdateID).Returns(updateId1);
            //Mock.Get(update2).Setup(u => u.Identity.UpdateID).Returns(updateId2);

            //var updateCollection = new UpdateCollectionClass();
            //updateCollection.Add(update1);
            //updateCollection.Add(update2);

            //var updateClient = Mock.Of<IUpdateClient>();
            //Mock.Get(updateClient).Setup(u => u.GetAvailableUpdates()).Returns(updateCollection);

            //var installationResult = Mock.Of<IInstallationResult>();
            //Mock.Get(installationResult).Setup(r => r.RebootRequired).Returns(true);

            //Mock.Get(updateClient).Setup(u => u.InstallUpdates(updateCollection)).Returns(new Result()
            //{
            //    ProcessedUpdates = new UpdateCollectionClass(),
            //    InstallationResult = installationResult,
            //    NotProcessedUpdates = new UpdateCollectionClass(),
            //    Reasons = new string[] { }
            //});

            //var taskHandler = Mock.Of<ITaskHandler>();

            //var httpClient = getHttpClientWithNoExcludedUpdates();
            //mController = createController(null, httpClient, updateClient, null, taskHandler);

            //bool inactive = false;
            //mController.InactiveStateActivated += (s, e) => { inactive = true; };
            //mController.BeginUpdate();

            //while (!inactive)
            //{
            //    Thread.Sleep(0);
            //}

            //Mock.Get(taskHandler).Verify(t => t.RebootSystem(), Times.Once);
        }

        [TestMethod]
        public void Given_UpdatesInstalled_When_IsNotRebootNeeded_Then_RebootSystemIsNotCalled()
        {
            IUpdate update1 = Mock.Of<IUpdate>();
            IUpdate update2 = Mock.Of<IUpdate>();

            string updateId1 = "230b82d1-3abd-471a-a4f9-23f97fb857d9";
            string updateId2 = "8497fe19-9f28-4189-b671-b95d8ba8c2d9";

            Mock.Get(update1).Setup(u => u.Identity.UpdateID).Returns(updateId1);
            Mock.Get(update2).Setup(u => u.Identity.UpdateID).Returns(updateId2);

            var updateCollection = new UpdateCollectionClass();
            updateCollection.Add(update1);
            updateCollection.Add(update2);

            var updateClient = Mock.Of<IUpdateClient>();
            Mock.Get(updateClient).Setup(u => u.GetAvailableUpdates()).Returns(updateCollection);

            var installationResult = Mock.Of<IInstallationResult>();
            Mock.Get(installationResult).Setup(r => r.RebootRequired).Returns(false);
            Mock.Get(updateClient).Setup(u => u.InstallUpdates(updateCollection)).Returns(new Result()
            {
                InstallationResult = installationResult,
                UpdateInstallationLog = new UpdateInstallationLog()
            });

            var taskHandler = Mock.Of<ITaskHandler>();

            var httpClient = getHttpClientWithNoExcludedUpdates();
            mController = createController(null, httpClient, updateClient, null, taskHandler);

            bool inactive = false;
            mController.Inactivated += (s, e) => { inactive = true; };
            mController.BeginUpdate();
            while (!inactive) Thread.Sleep(0);

            Mock.Get(taskHandler).Verify(t => t.RebootSystem(), Times.Never);
        }

        [TestMethod]
        public void Given_TwoCallsOnBeginUpdate_WhenFirstCallInactivates_Then_SecondCallStarts()
        {
            var updateClient = Mock.Of<IUpdateClient>();
            Mock.Get(updateClient).Setup(u => u.GetAvailableUpdates()).Callback(() => Thread.Sleep(500)).Returns(new UpdateCollectionClass());

            mController = createController(null, null, updateClient);

            int k = 0;
            mController.Inactivated += (s, e) =>
            {
                if (k == 0)
                {
                    Mock.Get(updateClient).Verify(u => u.GetAvailableUpdates(), Times.Once);
                    k++;
                }
                else if (k == 1)
                {
                    Mock.Get(updateClient).Verify(u => u.InstallUpdates(It.IsAny<IUpdateCollection>()), Times.Never);
                    k++;
                }
            };

            mController.BeginUpdate();

            Thread.Sleep(100);

            var localConfig = Mock.Of<ILocalConfiguration>();
            Mock.Get(localConfig).Setup(lc => lc.GroupName).Returns("Not_present");

            var updateClientProperty = mController.GetType().GetProperty("LocalConfig", BindingFlags.Public | BindingFlags.Instance);
            updateClientProperty.SetValue(mController, localConfig);

            mController.BeginUpdate();

            while (k != 2)
            {
                Thread.Sleep(100);
            }
        }

        private CustomerConfigurationsFile getConfiguration()
        {
            var ci = new CultureInfo("nl-NL");
            var configItem = new CustomerConfiguration
            {
                CustomerName = CCustomerName,
                MaintenanceSchedule = new[]
                {
                    new TimeFrame
                    {
                        GroupName = CGroupName,
                        DayOfWeek = (int) DateTime.UtcNow.DayOfWeek,
                        WeekCount = (int) Math.Ceiling((decimal) DateTime.UtcNow.Day / 7),
                        StartTime = DateTime.UtcNow.AddHours(-1).ToString(ci),
                        EndTime = DateTime.UtcNow.AddHours(1).ToString(ci)
                    }
                },
                ServerUpdateConfiguration = new ServerConfiguration
                {
                    ExcludedServers = new[]
                    {
                        "ExcludedServer1", "ExcludedServer2"
                    },
                    Servers = new[]
                    {
                        new ServerConfigurationItem
                        {
                            Name = CServerName,
                            ExcludedUpdateKbArticles = new[] {"2267602", "2290967"}
                        }
                    }
                }
            };

            var config = new CustomerConfigurationsFile();
            config.Add(configItem);

            return config;
        }

        private void registerTypes()
        {
            InstanceFactory.RegisterSingleton<IScheduler, RetryScheduler>();
        }

    }
}
