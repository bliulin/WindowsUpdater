using System;
using Cegeka.Updater.Setup.CustomActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CustomActionTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var session = Mock.Of<ISessionWrapper>();
            Mock.Get(session).Setup(s => s.Get("TransientProperties0")).Returns("CUSTOMER_NAME;GROUP_NAME;CONFIG_FILE_URL;MONITORING_SERVICE_TEMPLATE_URL;CONNECTION_STRING");

            Mock.Get(session).Setup(s => s.Get("INSTALL_LOCATION")).Returns(@"d:\DEV\Cegeka.Updater\Setup\Assemblies\");
            Mock.Get(session).Setup(s => s.Get("CUSTOMER_NAME")).Returns("My customer");
            Mock.Get(session).Setup(s => s.Get("GROUP_NAME")).Returns("Development");
            Mock.Get(session).Setup(s => s.Get("CONFIG_FILE_URL")).Returns("http://example.com/file.xml");
            Mock.Get(session).Setup(s => s.Get("MONITORING_SERVICE_TEMPLATE_URL")).Returns("https://mon.cegeka.be/remote_maintenance/?token=token&reason=reason&hostname={0}&action=action");
            Mock.Get(session).Setup(s => s.Get("CONNECTION_STRING")).Returns("constr");

            CustomActions.ExecuteDumpPropertiesToCustomActiondata(session);
            CustomActions.ExecuteModifyConfigurationFile(session);
        }
    }
}
