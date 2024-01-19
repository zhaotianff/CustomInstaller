using CustomInstaller.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CustomInstaller
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static SetupType SetupType = SetupType.Install;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length > 0 && e.Args[0].ToUpper() == nameof(SetupType.UnInstall).ToUpper())
                SetupType = SetupType.UnInstall;
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show($"安装发生错误[{e.Exception.Message}]");
            CustomInstaller.Logger.Logger.Error(e.Exception.Message);
        }
    }
}
