using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core
{
    /// <summary>
    /// Builder options class for selecting what to include
    /// </summary>
    public class BuilderOptions
    {
        /// <summary>
        /// GUID of plugin. Use PluginLoader class to get a list of plugins
        /// </summary>
        public List<Guid> Plugins = new List<Guid>();
        /// <summary>
        /// The output file
        /// </summary>
        public string Output { get; set; } = "";
        public BuilderOptionsOutputType OutputType { get; set; } = BuilderOptionsOutputType.VHD;

        /// <summary>
        /// BSOD Options
        /// </summary>
        public string? Emoticon { get; set; }
        public string? Str1 { get; set; }
        public string ?Str2 { get; set; }
        public string ?WebURL { get; set; }
        public string ?Support { get; set; }
        public string ?Fixes { get; set; }
        public bool AeroBSOD { get; set; }
    }
}
