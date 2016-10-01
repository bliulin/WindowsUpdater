using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cegeka.Updater.Logic
{
    public class Constants
    {
        public const string CEventLogDescription = "Cegeka automatic patch management";
        public const string CEventLogFailedToRetrieveCentralConfiguration = "Failed to retrieve the central configuration file.";
        public const string CEventLogFailedToWriteToDatabase = "Failed to write installation report in the database.";
        public const string CMessageUpdateNotDownloaded = "Update was not downloaded.";
        public const string CMessageUpdateCanRequestUserInput = "Update can request user input and it will not be installed.";
        public const string CMessageExcluded = "excluded";

        public const string EventSourceName = "Cegeka Update Service";
        public const int CEventLogId = 187;
        
        public const int CRetryCount = 3;
        public const int CRetryTimeout = 30;
    }
}
