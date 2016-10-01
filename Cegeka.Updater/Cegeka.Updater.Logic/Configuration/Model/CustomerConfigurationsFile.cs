using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Cegeka.Updater.Logic.Configuration.Model
{
    [Serializable]
    [XmlRoot(Namespace="", ElementName="CustomerConfigurations")]
    public class CustomerConfigurationsFile : List<CustomerConfiguration>
    {
    }
}
