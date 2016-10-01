using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cegeka.Updater.Logic.Configuration;
using Cegeka.Updater.Logic.Reporting;
using Cegeka.Updater.Logic.Schedule;
using Cegeka.Updater.Logic.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cegeka.Updater.Tests
{
    [TestClass]
    public class SqlStatusReporterTest
    {
        [TestMethod]
        public void CommitTest()
        {
            var localConfig = Mock.Of<ILocalConfiguration>();
            Mock.Get(localConfig).Setup(c => c.DatabaseConnectionString).Returns("Server=.;Database=Reporting;User ID=sa;Password=sasa;Trusted_Connection=False;");
            Mock.Get(localConfig).Setup(c => c.CustomerName).Returns("Cegeka");

            var handler = Mock.Of<ITaskHandler>();
            Mock.Get(handler).Setup(c => c.GetMachineName()).Returns("cegeka-api");

            var reporter = new SqlStatusReporter(localConfig, handler);
            reporter.ReportStatus(new UpdateInstallationLogEntry("123", InstallationStatus.Failure, "It just failed.", DateTime.UtcNow));
            reporter.ReportStatus(new UpdateInstallationLogEntry("321", InstallationStatus.Success, "", DateTime.UtcNow));

            //reporter.Commit();
            var scheduler = new RetryScheduler();
            reporter.CommitWithRetry(scheduler);
        }
    }
}
