using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core.Plugins
{
    public class ModernTaskmgrPlugin : IPlugin
    {
        public Guid PluginGuid => Guids.ModernTaskmgr;

        public string DisplayName => "Add Modern Task Manager";

        public Guid[] Requires => new Guid[] { };

        public void Run(PEModder modder)
        {
            modder.CopyFile("Windows/System32/en-us/Taskmgr.exe.mui");
            modder.CopyFile("Windows/System32/Taskmgr.exe");
            modder.CopyFile("Windows/System32/chartv.dll");
            modder.CopyFile("Windows/System32/D3DSCache.dll");
            modder.CopyFile("Windows/System32/NetworkUXBroker.dll");
            modder.CopyFile("Windows/System32/WlanMediaManager.dll");
            modder.CopyFile("Windows/System32/srumapi.dll");
            modder.CopyFile("Windows/System32/dxilconv.dll");
            modder.CopyFile("Windows/System32/LaunchTM.exe");
            modder.CopyFile("Windows/SystemResources/Taskmgr.exe.mun");
            modder.CopyFile("Windows/System32/pdh.dll");
        }
    }
}
