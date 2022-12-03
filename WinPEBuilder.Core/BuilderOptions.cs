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
        /// Adds DWM. Required for anything related to UWP
        /// </summary>
        public bool UseDWM { get; set; }
        /// <summary>
        /// Adds user profile system. Requires DWM 
        /// </summary>
        public bool UseLogonUI { get; set; }
        /// <summary>
        /// Adds StateReposotory services and SiHost. Requires LogonUI, DWM, and explorer.
        /// </summary>
        public bool EnableFullUWPSupport { get; set; }
        /// <summary>
        /// Installs all of the files for Explorer
        /// </summary>
        public bool UseExplorer { get; set; } = true;
        /// <summary>
        /// The output file
        /// </summary>
        public string Output { get; set; } = "";
        public BuilderOptionsOutputType OutputType { get; set; } = BuilderOptionsOutputType.VHD;
    }
}
