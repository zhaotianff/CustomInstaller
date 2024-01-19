using CustomInstaller.Model;
using CustomInstaller.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomInstaller
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskScheduler uiTaskScheduler;
        private SetupOption option;

        public MainWindow()
        {
            InitializeComponent();
            InitializeHardwareInformationCollect();
            InstallUtil.InitializeInstall(ReportProgress, ReportStatus);
            InitializeTaskScheduler();
            option = InitializeSetupOption();
            InitializeUI(option);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void btn_Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "浏览安装路径";

            if(folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.tbox_Path.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private async void btn_Install_Click(object sender, RoutedEventArgs e)
        {
            HideSetupUI();
            ShowProgressUI();
            option = GetSetupOptionFromUI(option);
            await InstallUtil.ExtractFileAsync(Properties.Resources.SetupFiles, option.InstallPath, 80f);
            CustomInstaller.Logger.Logger.Info("Extract file success.");
            CreateDirectory(option.CreateDirectory, option.InstallPath);
            CustomInstaller.Logger.Logger.Info("Create directory success.");
            await InstallUtil.InstallDependencyAsync(option.InstallPath, option.RuntimeDependencies);
            CustomInstaller.Logger.Logger.Info("Install dependency success.");
            var setupProperty = GetSetupProperty(option);
            await InstallUtil.CreateUninstallInRegistryAsync(setupProperty, 5f);
            CustomInstaller.Logger.Logger.Info("Create uninstall info success.");
            await InstallUtil.CacheInstallerAsync(setupProperty.InstallSource, option.InstallerName);
            CustomInstaller.Logger.Logger.Info("Cache package success");
            if (option.CreateDesktopShortcut && option.ShortcutItems?.Count > 0)
            {
                await InstallUtil.CreateDesktopShortcutWidthDelay(option.ShortcutItems.Select(x => option.InstallPath + "\\" + x.Name).ToList(), 5f, 1000);
                CustomInstaller.Logger.Logger.Info("Create desktop shortcut success");
            }

            if (option.CreateStartup && option.StartupItems?.Count > 0)
            {
                await InstallUtil.CreateStartupWidthDelay(option.ShortcutItems.Select(x => option.InstallPath + "\\" + x.Name).ToList(), 5f, 1000);
                CustomInstaller.Logger.Logger.Info("Create startup success");
            }

            CustomInstaller.Logger.Logger.Info("Install finish");
            ShowFinishUI();
        }

        private void HideSetupUI()
        {
            this.lbl_PathTitle.Visibility = Visibility.Collapsed;
            this.tbox_Path.Visibility = Visibility.Collapsed;
            this.btn_Browse.Visibility = Visibility.Collapsed;
            this.btn_Install.Visibility = Visibility.Collapsed;
            this.btn_Repair.Visibility = Visibility.Collapsed;
            this.btn_Remove.Visibility = Visibility.Collapsed;
        }

        private void ShowProgressUI()
        {
            this.progress.Visibility = Visibility.Visible;
            this.lbl_Status.Visibility = Visibility.Visible;
        }

        private void ShowInstallUI()
        {
            tbox_Path.Text = option.InstallPath;
        }

        private void ShowUnInstallUI()
        {
            this.lbl_PathTitle.Visibility = Visibility.Collapsed;
            this.tbox_Path.Visibility = Visibility.Collapsed;
            this.btn_Browse.Visibility = Visibility.Collapsed;
            this.btn_Install.Visibility = Visibility.Collapsed;

            this.btn_Repair.Visibility = Visibility.Visible;
            this.btn_Remove.Visibility = Visibility.Visible;
        }

        private void btn_Repair_Click(object sender, RoutedEventArgs e)
        {
            //在界面上点击修复时的操作
        }

        private async void btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            //在界面上点击移除时的操作
            var setupProperty = InstallUtil.GetUninstallFromRegistry(new SetupProperty());
            if (InstallUtil.CheckProgramStatus(setupProperty.InstallLocation))
            {
                MessageBox.Show("当前有程序正在执行，请手动关闭后再执行卸载...");
                return;
            }

            HideSetupUI();
            ShowProgressUI();

            await InstallUtil.UnInstallDependencyAsync(setupProperty.InstallLocation, option.RuntimeDependencies, 5f);
            CustomInstaller.Logger.Logger.Info("Uninstall dependency success");
            await InstallUtil.DeleteDirectoryAsync(setupProperty.InstallLocation, 80f);
            CustomInstaller.Logger.Logger.Info("Clear directory success");
            await InstallUtil.RemoveDesktopShortcutWidthDelay(option.ShortcutItems.Select(x => x.Name).ToList(), 5f, 1000);
            CustomInstaller.Logger.Logger.Info("Remove desktop shortcut success");
            await InstallUtil.RemoveStartupWidthDelay(option.ShortcutItems.Select(x => x.Name).ToList(), 5f, 1000);
            CustomInstaller.Logger.Logger.Info("Remove startup success");
            await InstallUtil.RemoveUninstallInRegistryAsync(setupProperty, 5f, 1000);
            CustomInstaller.Logger.Logger.Info("Remove registry success");
            CustomInstaller.Logger.Logger.Info("Uninstall finish");
            ShowFinishUI();
        }

        private async void InitializeHardwareInformationCollect()
        {
            await SystemInfoUtil.CollectHardwareInfoAsync();
        }

        private void InitializeTaskScheduler()
        {
            uiTaskScheduler = TaskScheduler.Current;
        }

        private SetupOption InitializeSetupOption()
        {
            option = LoadSetupConfig();
            return option;
        }

        private void InitializeUI(SetupOption setupOption)
        {
            if (App.SetupType == SetupType.Install)
            {
                //显示安装界面
                ShowInstallUI();
            }
            else
            {
                //显示卸载界面
                ShowUnInstallUI();
            }
        }

        private void btn_Finish_Click(object sender, RoutedEventArgs e)
        {
            if (option.AutoRunItems != null && App.SetupType == SetupType.Install)
            {
                InstallUtil.RunItemsAfterInstall(option.AutoRunItems, option.InstallPath);
            }
            this.Close();
        }

        private SetupProperty GetSetupProperty(SetupOption setupOption)
        {
            SetupProperty setupProperty = new SetupProperty();
            setupProperty.EstimatedSize = Properties.Resources.SetupFiles.Length / 1024;
            setupProperty.InstallLocation = setupOption.InstallPath;
            setupProperty.InstallSource = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Downloaded Installations\\{setupProperty.ProductId}";
            setupProperty.UninstallString = setupProperty.InstallSource + "\\" + setupOption.InstallerName + " uninstall";
            return setupProperty;
        }


        private void ShowFinishUI()
        {
            this.progress.Visibility = Visibility.Collapsed;
            this.lbl_Status.Visibility = Visibility.Collapsed;
            btn_Finish.Visibility = Visibility.Visible;
        }

        private void CreateDirectory(List<string> dirs, string installPath)
        {
            foreach (var dir in dirs)
            {
                var path = installPath + "\\" + dir;
                if (System.IO.Directory.Exists(path) == false)
                {
                    System.IO.Directory.CreateDirectory(path);
                    CustomInstaller.Logger.Logger.Info("Create directory:" + path);
                }
            }
        }


        public SetupOption GetSetupOptionFromUI(SetupOption setupOption)
        {
            //在界面上添加安装选项UI，然后从UI获取配置项
            setupOption.CreateDesktopShortcut = true;
            setupOption.CreateStartup = true;
            setupOption.InstallPath = this.tbox_Path.Text;
            return setupOption;
        }

        public SetupOption LoadSetupConfig()
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            SetupOption setupOption = serializer.Deserialize<SetupOption>(Properties.Resources.SetupConfig);
            return setupOption;
        }

        public void ReportProgress(float tick)
        {
            this.Dispatcher.Invoke(async () =>
            {
                var animationTick = tick / 10;
                for (int i = 0; i < 10; i++)
                {
                    this.progress.Value += animationTick;
                    await Task.Delay(50);
                }
            });
        }

        public void ReportStatus(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.lbl_Status.Content = msg;
            });
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
