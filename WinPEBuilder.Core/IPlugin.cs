using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core
{
    public interface IPlugin
    {
        /// <summary>
        /// The Plugin GUID
        /// </summary>
        public Guid PluginGuid { get; }
        /// <summary>
        /// The Plugin Display name
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// The GUID's of required plugins
        /// </summary>
        public Guid[] Requires { get; }
        /// <summary>
        /// Runs the plugin
        /// </summary>
        /// <param name="modder"></param>
        public void Run(PEModder modder);
    }
}
