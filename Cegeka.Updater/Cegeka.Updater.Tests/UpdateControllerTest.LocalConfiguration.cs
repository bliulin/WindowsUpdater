using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cegeka.Updater.Tests
{
    public partial class UpdateControllerTest
    {
        [TestMethod]
        public void Given_CustomerNameNotConfigured_Then_ThrowConfigurationException()
        {
            var config = getLocalConfig();
            Mock.Get(config).Setup(m => m.CustomerName).Equals(null);
            mController = createController(config);
            var run = mController.GetType().GetMethod("runUpdateProcedure", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                run.Invoke(mController, new object[] { });
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException is ConfigurationErrorsException);
            }
        }

        [TestMethod]
        public void Given_GroupNameNotConfigured_Then_ThrowConfigurationException()
        {
            var config = getLocalConfig();
            Mock.Get(config).Setup(m => m.GroupName).Equals(null);
            mController = createController(config);
            var run = mController.GetType().GetMethod("runUpdateProcedure", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                run.Invoke(mController, new object[] { });
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException is ConfigurationErrorsException);
            }
        }

        [TestMethod]
        public void Given_ServerUrlNotConfigured_Then_ThrowConfigurationException()
        {
            var config = getLocalConfig();
            Mock.Get(config).Setup(m => m.ConfigurationFileUrl).Equals(null);
            mController = createController(config);
            var run = mController.GetType().GetMethod("runUpdateProcedure", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                run.Invoke(mController, new object[] { });
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException is ConfigurationErrorsException);
            }
        }
    }
}
