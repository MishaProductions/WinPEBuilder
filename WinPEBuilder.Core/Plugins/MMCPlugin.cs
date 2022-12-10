using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core.Plugins
{
    public class MMCPlugin : IPlugin
    {
        public Guid PluginGuid => Guids.LogonUI;

        public string DisplayName => "Use MMC";

        public Guid[] Requires => new Guid[] { };

        public void Run(PEModder modder)
        {
            modder.CopyFile("Windows/System32/mmc.exe");
            modder.CopyFile("Windows/System32/mmcbase.dll");
            modder.CopyFile("Windows/System32/mmcndmgr.dll");
            modder.CopyFile("Windows/System32/en-us/mmc.exe.mui");
            modder.CopyFile("Windows/System32/en-us/mmcbase.dll.mui");
            modder.CopyFile("Windows/System32/en-us/mmcndmgr.dll.mui");

            //todo: why does mfc42loc.dll does not exist?
        }
    }
}
