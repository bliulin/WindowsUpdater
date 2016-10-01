using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Configuration.Install;

namespace Cegeka.Updater.Service
{
  [RunInstaller(true)]
  public partial class ProjectInstaller : Installer
  {
    public ProjectInstaller()
    {
      InitializeComponent();
    }

    protected override void OnBeforeInstall(IDictionary savedState)
    {
      base.OnBeforeInstall(savedState);
      if (savedState.Contains("ServiceName"))
      {
        this.CegekaUpdateServiceInstaller.ServiceName = savedState["ServiceName"].ToString();
      }
      var eventSourceName = "Cegeka Update Service";
      if (savedState.Contains("EventSourceName"))
      {
        eventSourceName = savedState["EventSourceName"].ToString();
      }

      if (!EventLog.SourceExists(eventSourceName))
      {
        EventLog.CreateEventSource(eventSourceName, "Application");
      }
    }
  }
}
