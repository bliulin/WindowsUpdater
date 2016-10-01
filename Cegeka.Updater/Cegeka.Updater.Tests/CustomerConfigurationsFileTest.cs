using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cegeka.Updater.Logic.Utils;
using Cegeka.Updater.Logic.Configuration.Model;
using System.IO;

namespace Cegeka.Updater.Tests
{
    [TestClass]
    public class CustomerConfigurationsFileTest
    {
        [TestMethod]
        public void TestDeserialization()
        {
            string configContent = File.ReadAllText(@"..\..\centralConfig.xml");
            var serializer = new Serialization<CustomerConfigurationsFile>();

            var obj = serializer.GetObjectFromXml(configContent);
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Count);
        }
    }
}
