using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core.Plugins
{
    public class LogonUIPlugin : IPlugin
    {
        public Guid PluginGuid => Guids.LogonUI;

        public string DisplayName => "Use LogonUI";

        public Guid[] Requires => new Guid[] { };

        public void Run(PEModder modder)
        {
            modder.CopyFile("Windows/System32/Windows.UI.Logon.dll");
            modder.CopyFile("Windows/System32/Windows.UI.XamlHost.dll");
            modder.CopyFile("Windows/System32/Windows.UI.Xaml.Controls.dll");
            modder.CopyFile("Windows/System32/Windows.UI.Xaml.Resources.21h1.dll");
            modder.CopyFile("Windows/System32/Windows.UI.Xaml.Phone.dll");
            modder.CopyFile("Windows/System32/Windows.UI.Xaml.Maps.dll");
            modder.CopyFile("Windows/System32/Windows.UI.Xaml.InkControls.dll");
            modder.CopyFile("Windows/System32/Windows.UI.Xaml.Resources.Common.dll");
            modder.CopyFile("Windows/System32/FontGlyphAnimator.dll");
            modder.CopyFile("Windows/System32/NetworkIcon.dll");
            modder.CopyFile("Windows/System32/shacct.dll");
            modder.CopyFile("Windows/System32/LanguageOverlayUtil.dll");
            modder.CopyFile("Windows/System32/threadpoolwinrt.dll");
            modder.CopyFile("Windows/System32/Windows.Devices.Sensors.dll");
            modder.CopyFile("Windows/System32/InputSwitch.dll");
            modder.CopyFile("Windows/System32/pfclient.dll");
            modder.CopyFile("Windows/System32/globinputhost.dll");
            modder.CopyFile("Windows/System32/Windows.UI.BioFeedback.dll");
            modder.CopyFile("Windows/System32/en-us/Windows.UI.Xaml.dll.mui");
            modder.CopyFile("Windows/System32/LogonUI.exe");
            // File.Copy(modder.SourcePath + "Windows/system32/cmd.exe", modder.Base + "Windows/system32/LogonUI.exe", true);
            File.Copy(modder.SourcePath + "Windows/system32/cmd.exe", modder.Base + "Windows/system32/UtilMan.exe", true);


            //user profile service
            modder.CopyFile("Windows/System32/profsvc.dll");
            modder.CopyFile("Windows/System32/profsvcext.dll");


            modder.CopyDir("Windows/SystemApps/Microsoft.LockApp_cw5n1h2txyewy/");
            modder.CopyDir("Windows/SystemResources/Windows.UI.Logon/");
            modder.CopyDir("Windows/SystemResources/Windows.UI.BioFeedback/");
            modder.CopyDir("ProgramData/Microsoft/User Account Pictures/");

            //enable verbose logon
            if (modder.SoftwareHive != null)
            {
                RegistryKey? key = modder.SoftwareHive.RootKey.CreateSubKey("Microsoft\\Windows\\CurrentVersion\\Policies\\System");
                if (key != null)
                {
                     key.SetValue("verbosestatus", 1, RegistryValueKind.DWord);
                    key.Close();
                }
                else
                {
                    modder.LogEvent("failed to enable verbose logon: registry key is null");
                }
            }
            else
            {
                modder.LogEvent("modder.SoftwareHive is null");
            }
        }
    }
}
