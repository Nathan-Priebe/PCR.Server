using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRServer
{
    class Common
    {
        //Logging errors to windows event log
        public static void LogError(string errorText)
        {
            using (var eventLog = new EventLog("Application"))
            {
                eventLog.Source = "PCRServer";
                eventLog.WriteEntry(errorText, EventLogEntryType.Error);
            }
        }
    }
}
