using ControlzEx.Theming;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ControlzEx.Theming;
using System.Windows.Media;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Windows.Media.Imaging;
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
            using FileStream openStream = File.OpenRead(saveConfigName);
            if (openStream != null)
            {
                DataModel? savedJson =  await JsonSerializer.DeserializeAsync<DataModel>(openStream);
                ThemeManager.Current.ChangeTheme(this, savedJson?.SerialTheme + "." + savedJson?.SerialColor);
            }
            else
            {
                // Set the application theme if no file is detected
                ThemeManager.Current.ChangeTheme(this, "Dark.Purple");
            }
        }
    }
}
