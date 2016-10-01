using System.Diagnostics;

namespace Cegeka.Updater.Logic.Utils
{
    public interface ITaskHandler
    {
        void WriteEventLog(int eventLogId, string description);

        void WriteEventLog(int eventLogId, EventLogEntryType eventLogEntryType, string description);

        void CallMonitoringApi();

        string GetMachineName();

        void RebootSystem();
    }
}