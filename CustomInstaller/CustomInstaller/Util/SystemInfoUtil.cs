using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CustomInstaller.Logger.Logger;

namespace CustomInstaller.Util
{
    public class SystemInfoUtil
    {
        private static void CollectHardwareInfo()
        {
            try
            {
                Info("******************************************");
                Info(DateTime.Now.ToString());
                Info("Collecting hardware infomation............");
                CollectOperatingSystem();
                CollectCpuInfo();
                CollectMemory();
                CollectUser();
                Info("******************************************");
            }
            catch
            {

            }
        }

        public async static Task CollectHardwareInfoAsync()
        {
            await TaskHelper.RunInThreadPool(() => CollectHardwareInfo());
        }


        private static void CollectOperatingSystem()
        {
            var caption = WmiUtil.GetWmiProperty("win32_OperatingSystem", "Caption");
            var architecture = WmiUtil.GetWmiProperty("win32_OperatingSystem", "OSArchitecture");
            Info("Operating System:" + caption);
            Info("Operating System Architecture:" + architecture);
        }

        private static void CollectCpuInfo()
        {
            var cpu = WmiUtil.GetWmiProperty("win32_processor", "Name");

            Info("CPU:" + cpu.ToString());
        }

        private static void CollectMemory()
        {
            var capacity = WmiUtil.GetWmiProperty("win32_physicalmemory", "Capacity");

            if(int.TryParse(capacity.ToString(),out int nCapacity))
            {
                int memorySize = nCapacity / 1024 / 1024;
                Info($"Memory : {memorySize} MB" );
            }    
        }

        private static void CollectUser()
        {
            var user = WmiUtil.GetWmiProperty("Win32_UserAccount", "Name");
            Info("User:" + user);
        }
    }
}
