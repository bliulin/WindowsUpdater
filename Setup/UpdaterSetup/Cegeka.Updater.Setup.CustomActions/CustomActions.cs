using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;

namespace Cegeka.Updater.Setup.CustomActions
{
    public static class CustomActions
    {
        private static string[] CUSTOMER_NAME = {"CUSTOMER_NAME", "CustomerName"};
        private static string[] GROUP_NAME = { "GROUP_NAME", "GroupName" };
        private static string[] CONFIG_FILE_URL = { "CONFIG_FILE_URL", "ConfigurationFileUrl" };
        private static string[] MONITOR_WEBSERVICE_URL = { "MONITORING_SERVICE_TEMPLATE_URL", "MonitoringServiceTemplateUrl" };
        private static string[] CONNECTION_STRING = { "CONNECTION_STRING", "StatusReportingServer" };

        [CustomAction]
        public static ActionResult ModifyConfigurationFile(Session session)
        {
            try
            {
                session.Log("Start ModifyLiveConfigurationFile.");

                CUSTOMER_NAME[1] = session[CUSTOMER_NAME[1]];
                GROUP_NAME[1] = session[GROUP_NAME[1]];
                CONFIG_FILE_URL[1] = session[CONFIG_FILE_URL[1]];
                MONITOR_WEBSERVICE_URL[1] = session[MONITOR_WEBSERVICE_URL[1]];
                CONNECTION_STRING[1] = session[CONNECTION_STRING[1]];

                string installLocation = session["INSTALL_LOCATION"];
                string configFilePath = Path.Combine(installLocation, "Cegeka.Updater.Service.exe.config");
                
                var doc = XDocument.Load(configFilePath);
                var appSettings = doc.Element("appSettings");
                
                replaceAppSettingAttributeValue(appSettings, CUSTOMER_NAME[0], CUSTOMER_NAME[1]);
                replaceAppSettingAttributeValue(appSettings, GROUP_NAME[0], GROUP_NAME[1]);
                replaceAppSettingAttributeValue(appSettings, CONFIG_FILE_URL[0], CONFIG_FILE_URL[1]);
                replaceAppSettingAttributeValue(appSettings, MONITOR_WEBSERVICE_URL[0], MONITOR_WEBSERVICE_URL[1]);

                var connectionStringSection = doc.Element("connectionStrings");
                replaceConnectionString(connectionStringSection, CONNECTION_STRING[0], CONNECTION_STRING[1]);

                doc.Save(configFilePath);
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
            System.Diagnostics.Debugger.Launch();
            Debugger.Break();

            string transientProperties = getTransientProperties(session);
            string[] parts = transientProperties.Split(';');
            var sb = new StringBuilder();

            foreach (string key in parts)
            {
                if (String.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                string value = session[key];
                if (null != value)
                {
                    value = value.Replace(";", @"\;");
                }
                else
                {
                    value = String.Empty;
                }
                sb.AppendFormat("{0}={1};", key, value);
            }

            session["CUSTOM_ACTION_DATA"] = sb.ToString();
            return ActionResult.Success;
        }

        private static string getTransientProperties(Session session)
        {
            int index = 0;
            string key = String.Format("TransientProperties{0}", index);
            string value = String.Empty;
            bool end = false;
            do
            {
                string currentValue = session[key];
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
            connectionStringSection.Elements("add").FirstOrDefault(el => el.Attribute("key").Value == connectionStringName).Attribute("value").SetValue(connectionStringValue);
        }
    }
}
