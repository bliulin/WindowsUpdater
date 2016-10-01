using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cegeka.Updater.Logic.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cegeka.Updater.Tests
{
    [TestClass]
    public class UpdateInstallationLogTest
    {
        [TestMethod]
        public void SaveReportTest()
        {
            var log = new UpdateInstallationLog();
            log.Add(new UpdateInstallationLogEntry("123", InstallationStatus.Success, "", DateTime.UtcNow));
            log.Add(new UpdateInstallationLogEntry("321", InstallationStatus.Failure, "it failed.", DateTime.UtcNow));

            var rs = new ReportStorage();
            string filePath = rs.SaveReportLog(log);

            Assert.IsTrue(File.Exists(filePath));
        }
    }
}
