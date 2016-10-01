using System;
using Cegeka.Updater.Logic.Schedule;

namespace Cegeka.Updater.Logic.Reporting
{
    public interface IStatusReporter
    {
        void ReportStatus(UpdateInstallationLogEntry logEntry);
        bool Commit();
        bool CommitWithRetry(IScheduler scheduler);
    }
}
