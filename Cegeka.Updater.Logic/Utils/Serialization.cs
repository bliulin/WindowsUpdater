using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Cegeka.Updater.Logic.Utils
{
  public class Serialization<T> where T : class
  {
    public T GetObjectFromXml(string xmlData)
    {
      var xmlSerializer = new XmlSerializer(typeof (T));
      var result = xmlSerializer.Deserialize(new XmlTextReader(new StringReader(xmlData)));

      return result as T;
    }

    public string Serialize(T instance)
    {
      var xmlSerializer = new XmlSerializer(typeof (T));

      var writer = new StringWriter();
      xmlSerializer.Serialize(writer, instance);

      return writer.ToString();
    }
  }
}
