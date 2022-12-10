using ControlzEx.Theming;
using System.IO;
using System.Text.Json;
using System.Windows;
using WinPEBuilder.WPF.Configuration;

namespace WinPEBuilder.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Theme settings deserialization
            string saveConfigName = "Usersconfiguration.json";
            try
            {
                using FileStream openStream = File.OpenRead(saveConfigName);
                if (openStream != null)
                {
                    DataModel? savedJson = await JsonSerializer.DeserializeAsync<DataModel>(openStream);
                    ThemeManager.Current.ChangeTheme(this, savedJson?.SerialTheme + "." + savedJson?.SerialColor);
                }
                else
                {
                    // Set the application theme if no file is detected
                    ThemeManager.Current.ChangeTheme(this, "Dark.Purple");
                }
            }
            catch
            {
                // Set the application theme if no file is detected
                ThemeManager.Current.ChangeTheme(this, "Dark.Purple");
            }
        }
    }
}
