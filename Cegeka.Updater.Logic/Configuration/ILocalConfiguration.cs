
namespace Cegeka.Updater.Logic.Configuration
{
    public interface ILocalConfiguration
    {
        string CustomerName { get; }
        string GroupName { get; }
        string ConfigurationFileUrl { get; }
        string DatabaseConnectionString { get; }
        string MonitoringServiceTemplateUrl { get; }
        string MonitoringServiceHost { get; }

        int ReadConfigFileRetryCount { get; }
    }
}