using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PCR.Server.Data
{
    public class PCRSystem
    {
        public static string version()
        {
            return  Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
                                Assembly.GetExecutingAssembly().GetName().Version.Minor + "." +
                                Assembly.GetExecutingAssembly().GetName().Version.Build;
        }
    }
}
