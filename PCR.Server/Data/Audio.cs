using System;
using System.Collections.Generic;
using System.Diagnostics;
using PCR.Server.Models;
using PCRServer;
using PCR.Common.Models;

namespace PCR.Server.Data
{
    public class Audio
    {
        public static string UpdateVolume(Message message)
        {
            if (message.Volume < 0)
            {
                message.Volume = 0;
            }

            if (message.Volume > 100)
            {
                message.Volume = 100;
            }

            if (message.ProcessId != 0)
            {
                //loop through all active audio processes
                foreach (var ctl in AudioLibrary.EnumerateApplications())
                {
                    ctl.GetProcessId(out UInt32 pId);
                    //Find required audio process and adjust volume
                    if (pId == message.ProcessId)
                    {
                        AudioLibrary.SetApplicationVolume(ctl, message.Volume);
                        break;
                    }
                }
            }
            else
            {
                AudioLibrary.SetMasterVolume(message.Volume);
            }
            
            return message.Volume.ToString();
        }

        public static List<AppDetails> AllApps()
        {
            var appDetailsList = new List<AppDetails>();

            //Create master volume object
            var masterDetails = new AppDetails();
            masterDetails.App = "Master Volume";
            masterDetails.Volume = AudioLibrary.GetMasterVolume();
            masterDetails.Mute = AudioLibrary.MasterMuteState();
            masterDetails.ProcessId = 0;
            appDetailsList.Add(masterDetails);

            //Find all other active audio processes
            foreach (var ctl in AudioLibrary.EnumerateApplications())
            {
                var appDetails = new AppDetails();
                ctl.GetDisplayName(out string dn);
                ctl.GetProcessId(out uint retval);
                //Process Id 0 is system sounds, currently being ignored until can be handled properly
                if (retval == 0)
                {
                    continue;
                }
                appDetails.ProcessId = retval;
                //If audio process description does not exist, use the app process name
                if (string.IsNullOrEmpty(dn))
                {
                    var p = Process.GetProcessById(Convert.ToInt32(retval));
                    appDetails.App = p.MainWindowTitle;
                }
                else
                {
                    appDetails.App = dn;
                }
                appDetails.Volume = AudioLibrary.GetApplicationVolume(ctl);
                appDetails.Mute = AudioLibrary.AppMuteState(ctl);
                appDetailsList.Add(appDetails);
            }
            return appDetailsList;
        }

        public static void MuteApp(int processId)
        {
            if (processId < 0)
            {
                throw new Exception("Proccess must have an ID value greater than 0");
            }

            if (processId != 0)
            {
                foreach (var ctl in AudioLibrary.EnumerateApplications())
                {
                    ctl.GetProcessId(out uint pId);
                    if (pId == processId)
                    {
                        AudioLibrary.MuteApp(ctl);
                        break;
                    }
                }
            }
            else
            {
                AudioLibrary.MuteMaster();
            }
        }
    }
}
