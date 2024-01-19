using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomInstaller.Model
{
    public class SetupOption
    {
        public string InstallTitle { get; set; }

        public string UnInstallTitle { get; set; }

        public string Icon { get; set; }

        public string ReleaseNoteTitle { get; set; }

        public string InstallPath { get; set; }

        public string InstallerName { get; set; }

        public bool CreateDesktopShortcut { get; set; }

        public bool CreateStartup { get; set; }

        public List<string> CreateDirectory { get; set; }

        public List<NameArgsMap> ShortcutItems { get; set; }

        public List<NameArgsMap> StartupItems { get; set; }

        public List<NameArgsMap> AutoRunItems { get; set; }

        public List<RuntimeDependency> RuntimeDependencies { get; set; }

    }

    public class RuntimeDependency
    {
        public string DependencyName { get; set; }
        public DependencyType DependencyType { get; set; }
        public string Path { get; set; }
        public string InstallArgs { get; set; }
        public string UnInstallArgs { get; set; }
        public string FriendlyName { get; set; }
    }

    public class NameArgsMap
    {
        public string Name { get; set; }

        public string Args { get; set; }
    }

    public enum DependencyType
    {
        Command,
        MsiInstaller,
        ExeInstaller
    }
}
