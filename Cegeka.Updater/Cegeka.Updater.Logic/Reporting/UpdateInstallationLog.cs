using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WUApiLib;

namespace Cegeka.Updater.Logic.Reporting
{
    [Serializable]
    public class UpdateInstallationLog : List<UpdateInstallationLogEntry>
    {
        public static UpdateInstallationLog Create(IEnumerable<UpdateInstallationLogEntry> updateLogEntries)
        {
            var instance = new UpdateInstallationLog();
            instance.AddRange(updateLogEntries);
            return instance;
        }

        public static IEnumerable<UpdateInstallationLogEntry> CreateExcludedLogEntries(UpdateCollectionClass updates)
        {
            var entries = new List<UpdateInstallationLogEntry>();
            for (int i = 0; i < updates.Count; i++)
            {
                var update = updates[i];
                var entry = new UpdateInstallationLogEntry(update.KBArticleIDs[0], InstallationStatus.NotAttempted, Constants.CMessageExcluded, DateTime.UtcNow);
                entries.Add(entry);
            }
            return entries;
        }
    }
}
