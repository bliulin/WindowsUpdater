using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cegeka.Updater.Logic.Reporting
{
    public interface IReportStorage
    {
        string SaveReportLog(UpdateInstallationLog updateLog);
        UpdateInstallationLog LoadReportLog();
        void DeleteReports();
    }
}
