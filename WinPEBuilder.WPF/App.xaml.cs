using ControlzEx.Theming;
using System.IO;
using System.Text.Json;
using System.Windows;
using WinPEBuilder.WPF.Configuration;
using WinPEBuilder.WPF.Configuration.Configuration;

namespace WinPEBuilder.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.Current.ChangeTheme(this, Settings.Data.SerialTheme + "." + Settings.Data.SerialColor);
        }
    }
}
