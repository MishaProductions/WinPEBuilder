using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WinPEBuilder.WPF.Configuration.Configuration
{
    internal class DataModel
    {
        public string SerialTheme { get; set; } = "Dark";
        public string SerialColor { get; set; } = "Purple";

        public string VHDPath { get; set; } = "";
        public string ISOSourcePath { get; set; } = "";
    }
}
