using System.Diagnostics;

namespace PCR.Server
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
