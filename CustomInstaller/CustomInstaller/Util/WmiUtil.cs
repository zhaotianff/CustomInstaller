using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace CustomInstaller.Util
{
    public class WmiUtil
    {
        public static string GetWmiProperty(string wmiProvider,string propertyName)
        {
            ManagementClass managementClass = new ManagementClass(wmiProvider);
            ManagementObjectCollection moc = managementClass.GetInstances();
           
            foreach (ManagementObject managementObject in moc)
            {
                return managementObject[propertyName].ToString();
            }
            return "";
        }
    }
}
