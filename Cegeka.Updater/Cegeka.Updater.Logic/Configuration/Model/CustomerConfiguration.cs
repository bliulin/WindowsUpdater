using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Cegeka.Updater.Logic.Configuration.Model
{
    [Serializable]
    public class CustomerConfiguration
    {
        [XmlAttribute("customerName")]
        public string CustomerName { get; set; }

        [XmlArray("MaintenanceSchedule")]
        [XmlArrayItem(ElementName = "TimeFrame")]
        public TimeFrame[] MaintenanceSchedule { get; set; }

        [XmlElementAttribute("ServerConfiguration")]
        public ServerConfiguration ServerUpdateConfiguration { get; set; }
    }
}
