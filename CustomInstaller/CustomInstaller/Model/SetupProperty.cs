using CustomInstaller.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomInstaller.Model
{
    public class SetupProperty
    {
        public string ProductId => "{DEF97942-13F2-4E38-A5BD-D7890F0A9DF8}";
        public string Comments => "自定义安装包演示";
        public string Contact => "";
        public string DisplayIcon => System.Reflection.Assembly.GetExecutingAssembly().Location;
        public string DisplayName => "自定义安装包演示";
        public string DisplayVersion => VersionUtil.GetVersionString();
        public int EstimatedSize { get; set; }
        public string HelpLink => "https://github.com/zhaotianff";
        public string InstallDate => DateTime.Now.ToString();
        public string InstallLocation { get; set; }
        public string InstallSource { get; set; }
        public string UninstallString { get; set; }
        public string Publisher => "zhaotianff";
    }
}
