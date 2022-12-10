using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core.Plugins
{
    public class ExplorerPlugin : IPlugin
    {
        public Guid PluginGuid => Guids.Explorer;

        public string DisplayName => "Use Explorer Shell";

        public Guid[] Requires => new Guid[] { };

        public void Run(PEModder modder)
        {
            modder.CopyFile("Windows/System32/twinapi.dll");
            modder.CopyFile("Windows/System32/en-us/twinapi.dll.mui");
            modder.CopyFile("Windows/System32/twinapi.appcore.dll");
            modder.CopyFile("Windows/System32/en-us/twinapi.appcore.dll.mui");

            modder.CopyFile("Windows/System32/twinui.dll");
            modder.CopyFile("Windows/System32/en-us/twinui.dll.mui");
            modder.CopyFile("Windows/System32/twinui.appcore.dll");
            modder.CopyFile("Windows/System32/en-us/twinui.appcore.dll.mui");

            modder.CopyFile("Windows/System32/cscui.dll");
            modder.CopyFile("Windows/System32/NetworkExplorer.dll");
            modder.CopyFile("Windows/System32/en-us/NetworkExplorer.dll.mui");
            modder.CopyFile("Windows/explorer.exe");
            modder.CopyFile("Windows/en-us/explorer.exe.mui");
            modder.CopyFile("Windows/System32/rmclient.dll");
            modder.CopyFile("Windows/System32/en-US/RmClient.exe.mui");
            modder.CopyFile("Windows/System32/Windows.Globalization.dll");
            modder.CopyFile("Windows/System32/Windows.System.Launcher.dll");
            modder.CopyFile("Windows/System32/windowsudk.shellcommon.dll");
            modder.CopyFile("Windows/System32/windowsudkservices.shellcommon.dll");
            modder.CopyFile("Windows/System32/efswrt.dll");
            modder.CopyFile("Windows/System32/taskschd.dll");
            modder.CopyFile("Windows/System32/Windows.UI.dll");
            modder.CopyFile("Windows/System32/Windows.UI.XAML.dll");
            modder.CopyFile("Windows/System32/Windows.StateRepositoryClient.dll");
            modder.CopyFile("Windows/System32/Windows.StateRepositoryCore.dll");
            modder.CopyFile("Windows/System32/StateRepository.Core.dll");
            modder.CopyFile("Windows/System32/Windows.StateRepository.dll");
            modder.CopyFile("Windows/System32/IDStore.dll");
            modder.CopyFile("Windows/System32/appresolver.dll");
            modder.CopyFile("Windows/System32/desk.cpl");
            modder.CopyFile("Windows/System32/control.exe");
            modder.CopyFile("Windows/System32/WinLangdb.dll");
            modder.CopyFile("Windows/System32/StartTileData.dll");
            modder.CopyFile("Windows/System32/mydocs.dll");
            modder.CopyFile("Windows/System32/Windows.Storage.Search.dll");
            modder.CopyFile("Windows/System32/CloudExperienceHostCommon.dll");
            modder.CopyFile("Windows/System32/TaskFlowDataEngine.dll");
            modder.CopyFile("Windows/System32/Windows.CloudStore.dll");
            modder.CopyFile("Windows/System32/mstask.dll");
            modder.CopyFile("Windows/System32/searchfolder.dll");
            modder.CopyFile("Windows/System32/en-us/searchfolder.dll.mui");
            modder.CopyFile("Windows/System32/OEMDefaultAssociations.dll");
            modder.CopyFile("Windows/System32/themeui.dll");
            modder.CopyFile("Windows/System32/amsi.dll");
            modder.CopyFile("Windows/System32/sscoreext.dll");
            modder.CopyFile("Windows/System32/Windows.StateRepositoryPS.dll");
            modder.CopyFile("Windows/System32/Windows.StateRepositoryUpgrade.dll");
            modder.CopyFile("Windows/System32/TileDataRepository.dll");
            modder.CopyFile("Windows/System32/twinui.pcshell.dll");
            modder.CopyFile("Windows/System32/Windows.UI.Immersive.dll");
            modder.CopyFile("Windows/System32/SndVolSSO.dll");
            modder.CopyFile("Windows/System32/shimgvw.dll");
            modder.CopyFile("Windows/System32/FirewallControlPanel.dll");
            modder.CopyFile("Windows/System32/wpnapps.dll");
            modder.CopyFile("Windows/System32/usodocked.dll");
            modder.CopyFile("Windows/System32/Windows.Management.Workplace.dll");
            modder.CopyFile("Windows/System32/wcmapi.dll");
            modder.CopyFile("Windows/System32/windows.immersiveshell.serviceprovider.dll");
            modder.CopyFile("Windows/System32/networkitemfactory.dll");
            modder.CopyFile("Windows/System32/dtsh.dll");
            modder.CopyFile("Windows/System32/en-us/dtsh.dll.mui");
            modder.CopyFile("Windows/System32/StructuredQuery.dll");
            modder.CopyFile("Windows/System32/ndfapi.dll");
            modder.CopyFile("Windows/System32/wdi.dll");
            modder.CopyFile("Windows/System32/fundisc.dll");
            modder.CopyFile("Windows/System32/clbcatq.dll");
            modder.CopyFile("Windows/SysWOW64/propsys.dll");

            modder.CopyService("StateRepository");
            Directory.CreateDirectory(modder.Base + "ProgramData/Microsoft/Windows/AppRepository");
            modder.CopyKey(HiveTypes.Software, "Microsoft\\Windows\\CurrentVersion\\AppX");
            modder.CopyKey(HiveTypes.Software, "Microsoft\\Windows\\CurrentVersion\\ShellCompatibility");
        }
    }
}
