using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Cegeka.Updater.Logic.Configuration.Model
{
  public class ServerConfiguration
  {
    [XmlArray("ExcludedServers")]
    [XmlArrayItem(ElementName = "Name")]
    public string[] ExcludedServers { get; set; }

    [XmlArray("Servers")]
    [XmlArrayItem("Server")]
    public ServerConfigurationItem[] Servers { get; set; }
  }
}
