using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Cegeka.Updater.Logic.Configuration.Model
{
  [Serializable]
  public class ServerConfigurationItem
  {
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlArray("ExcludedUpdates")]
    [XmlArrayItem("UpdateId")]
    public string[] ExcludedUpdateKbArticles { get; set; }
  }
}
