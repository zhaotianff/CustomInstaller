using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomInstaller.Util
{
    public class VersionUtil
    {
        public static bool IsWindows10OrGreater()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Version.Build >= 9200;
        }

        public static string GetVersionString()
        {
#pragma warning disable CS8602
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
#pragma warning restore CS8602
        }

        public static string GetBuildDateString()
        {
            return System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString("yyyy.MM.dd");
        }
    }
}
