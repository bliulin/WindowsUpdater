using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Cegeka.Updater.Logic.Reporting
{
    [Serializable]
    public class UpdateInstallationLogEntry
    {
        [XmlAttribute("kbid")]
        public string KbArticleId { get; set; }

        [XmlAttribute("status")]
        public InstallationStatus UpdateInstallationStatus { get; set; }

        [XmlAttribute("message")]
        public string Message { get; set; }

        [XmlAttribute("date")]
        public DateTime ProcessedTime { get; set; }

        public UpdateInstallationLogEntry(string kbArticleId, InstallationStatus status, string message, DateTime processedTime)
        {
            KbArticleId = kbArticleId;
            UpdateInstallationStatus = status;
            Message = message;
            ProcessedTime = processedTime;
        }

        public UpdateInstallationLogEntry()
        {            
        }
    }
}
