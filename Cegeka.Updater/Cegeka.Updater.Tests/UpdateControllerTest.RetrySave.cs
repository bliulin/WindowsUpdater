using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cegeka.Updater.Logic;
using Cegeka.Updater.Logic.Reporting;
using Cegeka.Updater.Logic.Schedule;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cegeka.Updater.Tests
{
    public partial class UpdateControllerTest
    {
        [TestMethod]
        public void Given_CouldNotReportStatus_Then_CommitWithRetryIsCalled()
        {
            var statusReporter = Mock.Of<IStatusReporter>();
            Mock.Get(statusReporter)
                .Setup(r => r.CommitWithRetry(It.IsAny<IScheduler>()))
                .Throws<Exception>();

            mController = createControllerWithUpdatesToInstall(statusReporter);

            bool inactivate = false;

            mController.Inactivated += (s, e) => { inactivate = true; };
            mController.BeginUpdate();

            while (!inactivate)
            {
                Thread.Sleep(0);
            }

            Mock.Get(statusReporter).Verify(r => r.CommitWithRetry(It.IsAny<IScheduler>()), Times.Once);
        }

        [TestMethod]
        public void Given_CouldNotReportStatus_Then_ReportIsSaved()
        {
            var statusReporter = Mock.Of<IStatusReporter>();
            Mock.Get(statusReporter)
                .Setup(r => r.CommitWithRetry(Mock.Of<IScheduler>()))
                .Throws<Exception>();

            var updateClient = getUpdateClientWithUpdatesToInstall(new[] { "123", "321" });
            mController = createController(null, null, updateClient, null, null, statusReporter);

            bool inactivate = false;

            mController.Inactivated += (s, e) => { inactivate = true; };
            mController.BeginUpdate();

            while (!inactivate)
            {
                Thread.Sleep(0);
            }

            Mock.Get(mController.ReportStorage).Verify(storage => 
                storage.SaveReportLog(It.Is<UpdateInstallationLog>(
                list => list.Count == 2 && list[0].KbArticleId == "123" && list[1].KbArticleId == "321")));
        }

        [TestMethod]
        public void Given_StatusReportedSuccessfully_Then_LoadSavedReportIsCalled()
        {
            var statusReporter = Mock.Of<IStatusReporter>();
            mController = createControllerWithUpdatesToInstall(statusReporter);

            bool inactivate = false;

            mController.Inactivated += (s, e) => { inactivate = true; };
            mController.BeginUpdate();

            while (!inactivate)
            {
                Thread.Sleep(0);
            }

            Mock.Get(mController.ReportStorage).Verify(storage=>storage.LoadReportLog(), Times.Once);
        }        

        [TestMethod]
        public void Given_SavedReportsSuccessfullySaved_Then_DeleteReportsIsCalled()
        {
            var reportStorage = Mock.Of<IReportStorage>();
            var savedReports = new UpdateInstallationLog
            {
                new UpdateInstallationLogEntry("999", InstallationStatus.Failure, "Some message", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0)))
            };
            Mock.Get(reportStorage).Setup(rs => rs.LoadReportLog()).Returns(savedReports);

            var reporter = Mock.Of<IStatusReporter>();
            Mock.Get(reporter).Setup(r => r.CommitWithRetry(It.IsAny<IScheduler>())).Returns(true);
            mController = createController(null, null, getUpdateClientWithUpdatesToInstall(new[] { "4269002", "2267602" }), null, null, reporter, reportStorage);

            bool inactivate = false;

            mController.Inactivated += (s, e) => { inactivate = true; };
            mController.BeginUpdate();

            while (!inactivate)
            {
                Thread.Sleep(0);
            }

            Mock.Get(reportStorage).Verify(s => s.DeleteReports(), Times.Once);
        }        
    }
}
