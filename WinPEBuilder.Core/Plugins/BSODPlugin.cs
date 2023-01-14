using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core.Plugins
{
    public class BSODPlugin : IPlugin
    {
        public Guid PluginGuid => Guids.BSOD;
        public string DisplayName => "Use a custom BSOD";
        public Guid[] Requires => new Guid[] { };
        
        public void Run(PEModder modder)
        {

        }
    }
}
