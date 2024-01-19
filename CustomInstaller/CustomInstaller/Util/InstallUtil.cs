using CustomInstaller.Model;
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace CustomInstaller.Util
{
    public class InstallUtil
    {
        private static Action<float> progressCallBack = null;
        private static Action<string> statusCallBack = null;

        public static void InitializeInstall(Action<float> progressFunc, Action<string> statusFunc)
        {
            progressCallBack = progressFunc;
            statusCallBack = statusFunc;
        }

        public static bool ExtractFile(byte[] fileBuffer,string rootDir,float progressCount)
        {
            try
            {
                IOUtil.CreateDir(rootDir);
                rootDir = IOUtil.CheckSlash(rootDir);

                using (MemoryStream ms = new MemoryStream(fileBuffer))
                {
                    var zipArchive = new ZipArchive(ms);

                    var tick = progressCount / zipArchive.Entries.Count;

                    foreach (var item in zipArchive.Entries)
                    {
                        IOUtil.CreateSubDir(rootDir, item.FullName);

                        if (item.FullName.EndsWith("/") == false)
                        {
                            var destFileName = rootDir + "\\" + item.FullName;
                            item.ExtractToFile(destFileName, true);
                            statusCallBack?.Invoke($"正在安装 {destFileName}");
                            Logger.Logger.Info("Extract:" + destFileName);
                        }

                        progressCallBack?.Invoke(tick);
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                Logger.Logger.Error(ex.Message);
                return false;
            }      
        }

        public async static Task<bool> ExtractFileAsync(byte[] fileBuffer, string rootDir, float progressCount)
        {
            return await TaskHelper.RunInThreadPoolBool(() => ExtractFile(fileBuffer, rootDir, progressCount));
        }

        public static void DeleteDirectory(string dir, float progressCount)
        {
            var dirInfo = new DirectoryInfo(dir);
            var fileEntryList = dirInfo.GetFileSystemInfos();
            foreach (var fileEntry in fileEntryList)
            {
                var tick = progressCount / fileEntryList.Length;
                try
                {
                    //if (fileEntry is DirectoryInfo)
                    //{
                    //    RecrusiveRemoveDir(fileEntry.FullName);
                    //    statusCallBack?.Invoke("删除文件夹 " + fileEntry.FullName);
                    //}

                    if (fileEntry is FileInfo)
                    {
                        (fileEntry as FileInfo).Delete();
                        statusCallBack?.Invoke("删除文件 " + fileEntry.FullName);
                        Logger.Logger.Info("Delete " + fileEntry.FullName);
                    }
                }
                catch(Exception ex)
                {
                    Logger.Logger.Error(ex.Message);
                    continue;
                }
                progressCallBack?.Invoke(tick);
            }
        }

        public async static Task DeleteDirectoryAsync(string dir, float progressCount)
        {
            await TaskHelper.RunInThreadPool(() => DeleteDirectory(dir, progressCount));
        }

        private static void RecrusiveRemoveDir(string dir)
        {
            try
            {
                var files = Directory.GetFiles(dir);
                if (files.Length == 0)
                    Directory.Delete(dir);
                else
                    files.ToList().ForEach(x => System.IO.File.Delete(x));

                var dirs = Directory.GetDirectories(dir);
                foreach (var subDir in dirs)
                {
                    RecrusiveRemoveDir(subDir);
                }
            }
            catch(Exception ex)
            {
                Logger.Logger.Error(ex.Message);
            }
        }

        public static void Regsvr32(string dllPath)
        {
            System.Diagnostics.Process.Start("regsvr32.exe", dllPath + " /s");
            statusCallBack?.Invoke($"-->注册组件完成:{dllPath}");
        }

        public static void CreateLnk(string exePath)
        {
            var exeName = System.IO.Path.GetFileNameWithoutExtension(exePath);
            var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + exeName + ".lnk";
            InternalCreateShortcut(exePath, shortcutPath);
            progressCallBack?.Invoke(5f);
        }

        public static void CreateStartup(string exePath,float progressCount)
        {
            var exeName = System.IO.Path.GetFileNameWithoutExtension(exePath);
            var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk";
            InternalCreateShortcut(exePath, shortcutPath);
            progressCallBack?.Invoke(progressCount);
        }

        private static void InternalCreateShortcut(string exePath, string shortcutPath)
        {
            try
            {
                WshShell shell = new WshShell();
                var exeName = System.IO.Path.GetFileNameWithoutExtension(exePath);
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(exePath);
                shortcut.WindowStyle = 1;
                shortcut.Description = exeName;
                shortcut.IconLocation = exePath + ",0";
                shortcut.Arguments = "";
                shortcut.Save();
            }
            catch (Exception ex)
            {
                Logger.Logger.Error(ex.Message);
            }
        }


        public static void MergeRegister(string regFilePath)
        {
            System.Diagnostics.Process.Start("regedit", " /s " + regFilePath);
            statusCallBack?.Invoke($"-->添加注册表完成");
        }

        public static void InstallMsi(string args)
        {
            var msiPath = "msiexec.exe";
            InternalInstall(msiPath, args);
        }

        public static void InstallCmd(string cmd)
        {
            var cmdPath = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\cmd.exe";
            InternalInstall(cmdPath, cmd);
        }

        public static void InstallExe(string exePath,string args)
        {
            if (!System.IO.File.Exists(exePath))
                return;

            InternalInstall(exePath, args);
        }

        private static void InternalInstall(string path,string args)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
            p.BeginErrorReadLine();
            p.WaitForExit();
            p.Close();
            p.Dispose();
        }

        public static void CreateUninstallInRegistry(SetupProperty setupProperty,float progressCount)
        {
            try
            {
                var productKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey($"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{setupProperty.ProductId}");

                foreach (var property in setupProperty.GetType().GetProperties())
                {
                    if (property.Name == nameof(setupProperty.ProductId))
                        continue;

                    if (property.Name == nameof(setupProperty.EstimatedSize))
                    {
                        productKey.SetValue(property.Name, property.GetValue(setupProperty), Microsoft.Win32.RegistryValueKind.DWord);
                    }
                    else
                    {
                        productKey.SetValue(property.Name, property.GetValue(setupProperty), Microsoft.Win32.RegistryValueKind.String);
                    }
                }

                progressCallBack?.Invoke(progressCount);
                statusCallBack?.Invoke("发布产品信息");
            }
            catch
            {

            }
        }

        public async static Task CreateUninstallInRegistryAsync(SetupProperty setupProperty, float progressCount)
        {
            await TaskHelper.RunInThreadPool(() => CreateUninstallInRegistry(setupProperty, progressCount), 1000);
        }

        public static void RemoveUninstallInRegistry(SetupProperty setupProperty, float progressCount)
        {
            try
            {
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKey($"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{setupProperty.ProductId}");
                progressCallBack?.Invoke(progressCount);
                statusCallBack?.Invoke("移除产品信息");
            }
            catch(Exception ex)
            {
                Logger.Logger.Error(ex.Message);
            }
        }

        public async static Task RemoveUninstallInRegistryAsync(SetupProperty setupProperty, float progressCount,int delayMillionSeconds)
        {
            await TaskHelper.RunInThreadPool(() => RemoveUninstallInRegistry(setupProperty, progressCount), delayMillionSeconds);
        }


        public static void CacheInstaller(string installSource,string installerName)
        {
            if (!Directory.Exists(installSource))
                Directory.CreateDirectory(installSource);

            try
            {
                System.IO.File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, installSource + "\\" + installerName, true);
                Logger.Logger.Info("Cache package to " + installSource + "\\" + installerName);
            }
            catch(Exception ex)
            {
                Logger.Logger.Error(ex.Message);
            }
        }

        public async static Task CacheInstallerAsync(string installSource, string installerName)
        {
            await TaskHelper.RunInThreadPool(() => CacheInstaller(installSource, installerName));
        }

        public static async Task InstallDependencyAsync(string installPath, List<RuntimeDependency> runtimeDependencies)
        {
            if (runtimeDependencies == null)
                return;

            foreach (var depencency in runtimeDependencies)
            {
                statusCallBack?.Invoke($"正在安装 {depencency.FriendlyName}");
                Logger.Logger.Info("Install " + depencency.FriendlyName);
                switch (depencency.DependencyType)
                {
                    case DependencyType.Command:
                        await TaskHelper.RunInThreadPool(() => { InstallCmd(depencency.InstallArgs); });
                        break;
                    case DependencyType.ExeInstaller:
                        await TaskHelper.RunInThreadPool(() => { InstallExe(installPath + "\\" + depencency.Path, depencency.InstallArgs); });
                        break;
                    case DependencyType.MsiInstaller:
                        var args = string.Format(depencency.InstallArgs, installPath + "\\" + depencency.Path);
                        await TaskHelper.RunInThreadPool(() => { InstallMsi(args); });
                        break;
                }
            }
        }

        public static async Task UnInstallDependencyAsync(string installPath, List<RuntimeDependency> runtimeDependencies,float progressCount)
        {
            if (runtimeDependencies == null)
                return;

            var tick = progressCount / runtimeDependencies.Count;

            foreach (var depencency in runtimeDependencies)
            {
                statusCallBack?.Invoke($"正在卸载 {depencency.FriendlyName}");
                Logger.Logger.Info("Uninstall " + depencency.FriendlyName);
                switch (depencency.DependencyType)
                {
                    case DependencyType.Command:
                        await TaskHelper.RunInThreadPool(() => { InstallCmd(depencency.UnInstallArgs); });
                        break;
                    case DependencyType.ExeInstaller:
                        await TaskHelper.RunInThreadPool(() => { InstallExe(installPath + "\\" + depencency.Path, depencency.UnInstallArgs); });
                        break;
                    case DependencyType.MsiInstaller:
                        var args = string.Format(depencency.UnInstallArgs, installPath + "\\" + depencency.Path);
                        await TaskHelper.RunInThreadPool(() => { InstallMsi(args); });
                        break;
                }

                progressCallBack?.Invoke(tick);
            }
        }

        public static async Task CreateDesktopShortcutWidthDelay(List<string> exeList,float progressCount,int delayMillionSeconds)
        {
            var tick = progressCount / exeList.Count;
            foreach (var item in exeList)
            {
                CreateLnk(item);
                progressCallBack?.Invoke(tick);
                statusCallBack?.Invoke("正在创建桌面快捷方式");
                await Task.Delay(delayMillionSeconds);
            }
        }

        public static async Task RemoveDesktopShortcutWidthDelay(List<string> exeList, float progressCount, int delayMillionSeconds)
        {
            var tick = progressCount / exeList.Count;
            foreach (var item in exeList)
            {
                var lnk = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + item.Replace(".exe",".lnk");

                if(System.IO.File.Exists(lnk))
                {
                    System.IO.File.Delete(lnk);
                    progressCallBack?.Invoke(tick);
                    statusCallBack?.Invoke("正在移除桌面快捷方式");
                    await Task.Delay(delayMillionSeconds);
                }   
            }
        }

        public static async Task CreateStartupWidthDelay(List<string> exeList,float progressCount,int delayMillionSeconds)
        {
            var tick = progressCount / exeList.Count;
            foreach (var item in exeList)
            {
                CreateStartup(item, tick); ;
                statusCallBack?.Invoke("正在创建开机启动项");
                await Task.Delay(delayMillionSeconds);
            }
        }

        public static async Task RemoveStartupWidthDelay(List<string> exeList,float progressCount,int delayMillionSeconds)
        {
            var tick = progressCount / exeList.Count;
            foreach (var item in exeList)
            {
                var lnk = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + item.Replace(".exe", ".lnk");

                if (System.IO.File.Exists(lnk))
                {
                    System.IO.File.Delete(lnk);
                    progressCallBack?.Invoke(tick);
                    statusCallBack?.Invoke("正在删除开机启动项");
                    await Task.Delay(delayMillionSeconds);
                }
            }
        }

        public static bool CheckProgramStatus(string currentDir)
        {
            foreach (var process in System.Diagnostics.Process.GetProcesses())
            {
                try
                {
                    if (process.ProcessName == System.Diagnostics.Process.GetCurrentProcess().ProcessName)
                        continue;

                    if (System.IO.Path.GetDirectoryName(process.MainModule.FileName) == currentDir)
                        return true;
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }

        public static SetupProperty GetUninstallFromRegistry(SetupProperty setupProperty)
        {
            try
            {
                var productKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey($"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{setupProperty.ProductId}");

                foreach (var property in setupProperty.GetType().GetProperties())
                {
                    if(property.CanWrite)
                    {
                        var value = productKey.GetValue(property.Name);
                        property.SetValue(setupProperty, value);
                    }

                }
            }
            catch
            {
                
            }

            return setupProperty;
        }

        public static void RunItemsAfterInstall(List<NameArgsMap> autoRunItems,string installPath)
        {
            Environment.CurrentDirectory = installPath;
            foreach (var runItems in autoRunItems)
            {
                var runPath = installPath + "\\" + runItems.Name;
                if (System.IO.File.Exists(runPath))
                    System.Diagnostics.Process.Start(runPath, runItems.Args);
            }
        }
    }
}
