using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cegeka.Updater.Logic.Utils;

namespace Cegeka.Updater.Logic.Reporting
{
    public class ReportStorage : IReportStorage
    {
        private const string FILENAME = "_report.xml";

        public string SaveReportLog(UpdateInstallationLog updateLog)
        {
            var s = new Serialization<UpdateInstallationLog>();

            var savedReports = LoadReportLog();
            savedReports.AddRange(updateLog);

            string xml = s.Serialize(savedReports);
            File.WriteAllText(FILENAME, xml);            

            return new FileInfo(FILENAME).FullName;
        }

        public UpdateInstallationLog LoadReportLog()
        {
            if (File.Exists(FILENAME))
            {
                var s = new Serialization<UpdateInstallationLog>();
                string xml = File.ReadAllText(FILENAME);
                var savedReports = s.GetObjectFromXml(xml);
                return savedReports;
            }

            return new UpdateInstallationLog();
        }

        public void DeleteReports()
        {
            File.Delete(FILENAME);
        }
    }
}
