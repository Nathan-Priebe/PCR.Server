using System.Reflection;

namespace PCR.Server.Data
{
    public class PcrSystem
    {
        public static string version()
        {
            return  Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
                                Assembly.GetExecutingAssembly().GetName().Version.Minor + "." +
                                Assembly.GetExecutingAssembly().GetName().Version.Build;
        }
    }
}
