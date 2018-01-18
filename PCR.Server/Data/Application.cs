using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PCR.Server.Models;
using System.Diagnostics;

namespace PCR.Server.Data
{
    class Application
    {
        public static List<Applications> AllApps()
        {
            List<Applications> items;
            using (var r = new StreamReader("localData.json"))
            {
                var json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<Applications>>(json);
            }
            return items;
        }

        public static void Execute(int appId)
        {
            List<Applications> items;
            using (var r = new StreamReader("localData.json"))
            {
                var json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<Applications>>(json);
            }

            var executeable = items.SingleOrDefault(x => x.AppId == appId);
            if(executeable != null) Process.Start(executeable.AppPath);
        }
    }
}
