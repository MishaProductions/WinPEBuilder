using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core.Plugins
{
    public class DWMPlugin : IPlugin
    {
        public Guid PluginGuid => Guids.DWM;

        public string DisplayName => "DWM Support";

        public Guid[] Requires => new Guid[] { };

        public void Run(PEModder modder)
        {
            modder.CopyFile("Windows/System32/dwm.exe");
            modder.CopyFile("Windows/System32/uDWM.dll");
            modder.CopyFile("Windows/System32/en-us/dwm.dll.mui");
            modder.CopyFile("Windows/System32/en-us/uDWM.dll.mui");
            modder.CopyFile("Windows/System32/en-us/dwminit.dll.mui");
            modder.CopyFile("Windows/System32/dwmscene.dll");
            modder.CopyFile("Windows/System32/dwmredir.dll");
            modder.CopyFile("Windows/System32/dwminit.dll");
            modder.CopyFile("Windows/System32/dwmcore.dll");
            modder.CopyFile("Windows/System32/dwmapi.dll");
            modder.CopyFile("Windows/System32/dcomp.dll");
            modder.CopyFile("Windows/System32/ism.dll");
            modder.CopyFile("Windows/System32/dxgi.dll");
            modder.CopyFile("Windows/System32/D3DCOMPILER_47.dll");
            modder.CopyFile("Windows/System32/dxcore.dll");
            modder.CopyFile("Windows/System32/d3d10warp.dll");
            modder.CopyFile("Windows/System32/directxdatabasehelper.dll");
            modder.CopyFile("Windows/System32/ResourcePolicyClient.dll");
            modder.CopyFile("Windows/System32/gameinput.dll");
            modder.CopyFile("Windows/System32/windows.applicationmodel.dll");
            modder.CopyFile("Windows/System32/d3d11.dll");
            modder.CopyFile("Windows/System32/WindowManagement.dll");
            modder.CopyFile("Windows/System32/windowmanagementapi.dll");
            modder.CopyFile("Windows/System32/wuceffects.dll");
            modder.CopyFile("Windows/apppatch/DirectXApps.sdb");
            modder.CopyFile("Windows/apppatch/DirectXApps_FOD.sdb");
            modder.CopyFile("Windows/System32/Windows.gaming.input.dll");
            modder.CopyFile("Windows/System32/DispBroker.Desktop.dll");
            modder.CopyFile("Windows/System32/DispBrokerDesktop.dll");
            modder.CopyFile("Windows/System32/DispBroker.dll");
            modder.CopyFile("Windows/System32/GameInputRedist.dll");
            modder.CopyFile("Windows/System32/dwmghost.dll");
            modder.CopyFile("Windows/System32/InputHost.dll");
            modder.CopyFile("Windows/System32/Windows.Graphics.dll");
            modder.CopyFile("Windows/System32/OneCoreUAPCommonProxyStub.dll");
            modder.CopyFile("Windows/System32/UIAutomationCore.dll");
            modder.CopyFile("Windows/System32/UIAnimation.dll");

            modder.CopyKey(HiveTypes.Software, "Microsoft\\Windows\\Dwm");
            modder.CopyKey(HiveTypes.Software, "Microsoft\\SecurityManager");
            modder.CopyKey(HiveTypes.Software, "Microsoft\\WindowsRuntime");
        }
    }
}
