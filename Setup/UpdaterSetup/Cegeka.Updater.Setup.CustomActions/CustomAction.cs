using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Contracten.BaseCA;
using Microsoft.Deployment.WindowsInstaller;

namespace Cegeka.Updater.Setup.CustomActions
{
    public static class CustomActions
    {
        private static string[] CUSTOMER_NAME = { "CUSTOMER_NAME", "CustomerName" };
        private static string[] GROUP_NAME = { "GROUP_NAME", "GroupName" };
        private static string[] CONFIG_FILE_URL = { "CONFIG_FILE_URL", "ConfigurationFileUrl" };
        private static string[] MONITOR_WEBSERVICE_URL = { "MONITORING_SERVICE_TEMPLATE_URL", "MonitoringServiceTemplateUrl" };
        private static string[] MONITOR_HOST = { "MONITORING_SERVICE_HOST", "MonitoringServiceHost" };
        private static string[] CONNECTION_STRING = { "CONNECTION_STRING", "StatusReportingServer" };

        [CustomAction]
        public static ActionResult ModifyConfigurationFile(Session session)
        {
            session.Log("Start ModifyLiveConfigurationFile.");

            try
            {
                ExecuteModifyConfigurationFile(getWrapper(session));
            }
            catch (Exception ex)
            {
                session.Log("Error in ModifyLiveConfigurationFile: " + ex.Message);
            }
            finally
            {
                session.Log("Finished ModifyLiveConfigurationFile.");
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult DumpPropertiesToCustomActionData(Session session)
        {
            //System.Diagnostics.Debugger.Launch();
            try
            {
                ExecuteDumpPropertiesToCustomActiondata(getWrapper(session));
            }
            catch (Exception ex)
            {
                session.Log(ex.Message);
                throw;
            }
            return ActionResult.Success;
        }

        public static void ExecuteModifyConfigurationFile(ISessionWrapper session)
        {
            //System.Diagnostics.Debugger.Launch();

            string customerName = session.Get(CUSTOMER_NAME[0]);
            string groupName = session.Get(GROUP_NAME[0]);
            string configFileUrl = session.Get(CONFIG_FILE_URL[0]);
            string monitorWebServiceUrl = session.Get(MONITOR_WEBSERVICE_URL[0]);
            string monitorHost = session.Get(MONITOR_HOST[0]);
            string connectionString = session.Get(CONNECTION_STRING[0]);
            string installLocation = session.Get("INSTALL_LOCATION");

            session.Log("CUSTOMER_NAME = " + customerName);
            session.Log("GROUP_NAME = " + groupName);
            session.Log("CONFIG_FILE_URL = " + configFileUrl);
            session.Log("MONITOR_WEBSERVICE_URL = " + monitorWebServiceUrl);
            session.Log("MONITOR_HOST = " + monitorHost);
            session.Log("CONNECTION_STRING = " + connectionString);
            session.Log("INSTALL_LOCATION" + installLocation);

            string configFilePath = Path.Combine(installLocation, "Cegeka.Updater.Service.exe.config");

            var doc = XDocument.Load(configFilePath);
            var appSettings = doc.Descendants().FirstOrDefault(e => e.Name == "appSettings");            

            replaceAppSettingAttributeValue(appSettings, CUSTOMER_NAME[1], customerName);
            replaceAppSettingAttributeValue(appSettings, GROUP_NAME[1], groupName);
            replaceAppSettingAttributeValue(appSettings, CONFIG_FILE_URL[1], configFileUrl);
            replaceAppSettingAttributeValue(appSettings, MONITOR_WEBSERVICE_URL[1], monitorWebServiceUrl);
            replaceAppSettingAttributeValue(appSettings, MONITOR_HOST[1], monitorHost);

            var connectionStringSection = doc.Descendants().FirstOrDefault(e => e.Name == "connectionStrings"); 
            replaceConnectionString(connectionStringSection, CONNECTION_STRING[1], connectionString);

            doc.Save(configFilePath);
        }

        public static void ExecuteDumpPropertiesToCustomActiondata(ISessionWrapper session)
        {
            string transientProperties = getTransientProperties(session);
            string[] parts = transientProperties.Split(';');
            var sb = new StringBuilder();

            foreach (string key in parts)
            {
                if (String.IsNullOrEmpty(key))
                {
                    continue;
                }

                string value = session.Get(key);
                if (!string.IsNullOrEmpty(value))
                {
                    value = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
                }

                sb.AppendFormat("{0}={1};", key, value);
            }

            session.Set("CUSTOM_ACTION_DATA", sb.ToString());
        }

        private static string getTransientProperties(ISessionWrapper session)
        {
            int index = 0;
            string key = String.Format("TransientProperties{0}", index);
            string value = String.Empty;
            bool end = false;
            do
            {
                string currentValue = session.Get(key);
                if (!String.IsNullOrEmpty(currentValue))
                {
                    if (!value.EndsWith(";"))
                    {
                        value = value + ";";
                    }

                    value += currentValue;
                    index++;
                    key = String.Format("TransientProperties{0}", index);
                }
                else
                {
                    end = true;
                }
            } while (!end);

            return value.Substring(1);
        }

        private static void replaceAppSettingAttributeValue(XElement appSettingsSection, string key, object value)
        {
            appSettingsSection.Elements("add").FirstOrDefault(el => el.Attribute("key").Value == key).Attribute("value").SetValue(value);
        }

        private static void replaceConnectionString(XElement connectionStringSection, string connectionStringName, string connectionStringValue)
        {
            connectionStringSection.Elements("add").FirstOrDefault(el => el.Attribute("name").Value == connectionStringName).Attribute("connectionString").SetValue(connectionStringValue);
        }

        private static ISessionWrapper getWrapper(Session session)
        {
            return new SessionWrapper(session);
        }
    }
}
