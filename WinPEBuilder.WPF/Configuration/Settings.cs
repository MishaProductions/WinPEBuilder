using ControlzEx.Theming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinPEBuilder.WPF.Configuration.Configuration;

namespace WinPEBuilder.WPF.Configuration
{
    internal class Settings
    {
        public static DataModel Data;
        public const string saveConfigName = "Usersconfiguration.json";
        static Settings()
        {
            if (!File.Exists(saveConfigName))
            {
                Data = new DataModel();
                Save();
            }

            //load the data
            var json = File.ReadAllText(saveConfigName);
            var x = JsonSerializer.Deserialize<DataModel>(json);

            x ??= new DataModel();

            Data = x;
        }

        public static void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(saveConfigName, JsonSerializer.Serialize(Data, options));
        }
    }
}
