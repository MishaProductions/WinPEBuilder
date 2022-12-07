using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static WinPEBuilder.Core.PEModder;

namespace WinPEBuilder.Core
{
    public class PEModder
    {
        /// <summary>
        /// Image base (eg Z:\)
        /// </summary>
        public string Base;
        /// <summary>
        /// Source Path (eg c:\mount\)
        /// </summary>
        public string SourcePath;

        public Hive? SoftwareHive;
        public Hive? SystemHive;
        public Hive? InstallSoftwareHive;
        public Hive? InstallSystemHive;
        public Builder Builder { get; private set; }
        public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        public PEModder(Builder builder)
        {
            Base = builder.ImagePath;
            SourcePath = builder.SourcePath;
            Builder = builder;
        }
        /// <summary>
        /// Copies a file to the image
        /// </summary>
        /// <param name="path">Windows/System32/shellstyle.dll</param>
        public void CopyFile(string path)
        {
            if (File.Exists(SourcePath + path))
            {
                //check if dest exists
                if (File.Exists(Base + path))
                {
                    TakeOwnership(Base + path, false);
                }
                try
                {
                    File.Copy(SourcePath + path, Base + path, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("error while copying file: " + ex.ToString());
                }

            }
            else
            {
                Debug.WriteLine("filenotfound: " + path);
            }
        }
        /// <summary>
        /// Copy registry key to the image
        /// </summary>
        /// <param name="software"></param>
        /// <param name="v"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CopyKey(HiveTypes type, string key)
        {
            Hive? source = null;
            Hive? dest = null;
            switch (type)
            {
                case HiveTypes.Software:
                    source = InstallSoftwareHive;
                    dest = SoftwareHive;
                    break;
                case HiveTypes.System:
                    source = InstallSystemHive;
                    dest = SystemHive;
                    break;
                case HiveTypes.Default:
                    throw new NotImplementedException();
                default:
                    break;
            }

            if (source == null || dest == null)
            {
                //this should not happen
                Debugger.Break();
                return;
            }

            var sourceKey = source.RootKey.OpenSubKey(key);
            if (sourceKey == null)
            {
                Debug.WriteLine("source key is null: " + key);
                Debugger.Break();
                return;
            }

            var destKey = dest.RootKey.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (destKey == null)
            {
                destKey = dest.RootKey.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (destKey == null)
                {
                    Debug.WriteLine("failed to create key: " + key);
                    Debugger.Break();
                    return;
                }
            }

            CopyKeyInternal(sourceKey, destKey);
            sourceKey.Close();
            destKey.Close();
        }

        private void CopyKeyInternal(RegistryKey sourceKey, RegistryKey destKey)
        {
            if (sourceKey is null)
            {
                throw new ArgumentNullException(nameof(sourceKey));
            }

            if (destKey is null)
            {
                throw new ArgumentNullException(nameof(destKey));
            }

            //set all values
            foreach (var name in sourceKey.GetValueNames())
            {
                var type = sourceKey.GetValueKind(name);
                var val = sourceKey.GetValue(name);
                if (type == RegistryValueKind.String || type == RegistryValueKind.ExpandString)
                {
                    if (val != null)
                        val = ((string)val).Replace(@"C:\", @"X:\");
                }
                else if (type == RegistryValueKind.MultiString)
                {
                    string[] t = (string[])val;
                    if (t != null)
                    {
                        for (int i = 0; i < t.Length; i++)
                        {
                            t[i] = t[i].Replace(@"C:\", @"X:\");
                        }
                        val = t;
                    }
                }

                destKey.SetValue(name, val, type);
            }

            //set default value
            var defvalue = sourceKey?.GetValue("");
            if (defvalue != null)
            {
                if (defvalue is string a)
                {
                    defvalue = a.Replace("C:\\", "X:\\");
                }
                else if (defvalue is string[] t)
                {
                    for (int i = 0; i < t.Length; i++)
                    {
                        ((string[])defvalue)[i] = t[i].Replace(@"C:\", @"X:\");
                    }
                }
                destKey.SetValue("", defvalue, sourceKey.GetValueKind(""));
            }

            //Copy subkeys
            foreach (var item in sourceKey.GetSubKeyNames())
            {
                RegistryKey? subkey = sourceKey.OpenSubKey(item);
                if (subkey != null)
                {
                    RegistryKey newdest = destKey.CreateSubKey(item, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    CopyKeyInternal(subkey, newdest);
                    subkey.Close();
                    newdest.Close();
                }
                else
                {
                    Debug.WriteLine("opensubkeyfailed: " + item);
                }
            }
        }
        public bool Run()
        {
            //needed for modern explorer
            CopyFile("Windows/System32/shellstyle.dll");
            Directory.CreateDirectory(Base + "Windows/Resources/Themes/Aero/Shell/");
            Directory.CreateDirectory(Base + "Windows/Resources/Themes/Aero/Shell/NormalColor/");
            CopyFile("Windows/Resources/Themes/Aero/Shell/NormalColor/ShellStyle.dll");
            CopyFile("Windows/System32/edputil.dll");
            CopyFile("Windows/System32/IconCodecService.dll");
            CopyFile("Windows/System32/apphelp.dll");
            CopyFile("Windows/System32/cscapi.dll");
            CopyFile("Windows/System32/atlthunk.dll");
            CopyFile("Windows/System32/policymanager.dll");
            CopyFile("Windows/System32/MrmCoreR.dll");
            CopyFile("Windows/System32/ActXPrxy.dll");
            CopyFile("Windows/System32/explorerframe.dll");
            CopyFile("Windows/System32/thumbcache.dll");
            CopyFile("Windows/System32/en-us/explorerframe.dll.mui");
            CopyFile("Windows/SystemResources/imageres.dll.mun");
            CopyFile("Windows/SystemResources/explorerframe.dll.mun");
            FixBlackScreen();
            //takeown config folder (required)
            TakeOwnership(SourcePath + "Windows/System32/config/", true);

            SoftwareHive = RegistyManager.MountHive(Base + "Windows/System32/config/SOFTWARE", "Tmp_Software");
            SystemHive = RegistyManager.MountHive(Base + "Windows/System32/config/System", "Tmp_System");

            InstallSoftwareHive = RegistyManager.MountHive(SourcePath + "Windows/System32/config/SOFTWARE", "Install_Software");
            InstallSystemHive = RegistyManager.MountHive(SourcePath + "Windows/System32/config/System", "Install_System");

            Builder.ReportProgress(false, 0, "Taking ownership of registry");
            Parallel.ForEach(new string[] { "Tmp_Software", "Tmp_System" }, delegate (string x)
            {
                RunSetACL(x);
            });

           
            if (Builder.Options.UseExplorer)
            {
                CopyFile("Windows/System32/twinapi.dll");
                CopyFile("Windows/System32/en-us/twinapi.dll.mui");
                CopyFile("Windows/System32/twinapi.appcore.dll");
                CopyFile("Windows/System32/en-us/twinapi.appcore.dll.mui");

                CopyFile("Windows/System32/twinui.dll");
                CopyFile("Windows/System32/en-us/twinui.dll.mui");
                CopyFile("Windows/System32/twinui.appcore.dll");
                CopyFile("Windows/System32/en-us/twinui.appcore.dll.mui");

                CopyFile("Windows/System32/cscui.dll");
                CopyFile("Windows/System32/NetworkExplorer.dll");
                CopyFile("Windows/System32/en-us/NetworkExplorer.dll.mui");
                CopyFile("Windows/explorer.exe");
                CopyFile("Windows/en-us/explorer.exe.mui");
                CopyFile("Windows/System32/rmclient.dll");
                CopyFile("Windows/System32/en-US/RmClient.exe.mui");
                CopyFile("Windows/System32/Windows.Globalization.dll");
                CopyFile("Windows/System32/Windows.System.Launcher.dll");
                CopyFile("Windows/System32/windowsudk.shellcommon.dll");
                CopyFile("Windows/System32/windowsudkservices.shellcommon.dll");
                CopyFile("Windows/System32/efswrt.dll");

                if (Builder.Options.EnableFullUWPSupport)
                {

                }
            }
            if (Builder.Options.UseDWM)
            {
                CopyFile("Windows/System32/dwm.exe");
                CopyFile("Windows/System32/uDWM.dll");
                CopyFile("Windows/System32/en-us/dwm.dll.mui");
                CopyFile("Windows/System32/en-us/uDWM.dll.mui");
                CopyFile("Windows/System32/en-us/dwminit.dll.mui");
                CopyFile("Windows/System32/dwmscene.dll");
                CopyFile("Windows/System32/dwmredir.dll");
                CopyFile("Windows/System32/dwminit.dll");
                CopyFile("Windows/System32/dwmcore.dll");
                CopyFile("Windows/System32/dwmapi.dll");
                CopyFile("Windows/System32/dcomp.dll");
                CopyFile("Windows/System32/ism.dll");
                CopyFile("Windows/System32/dxgi.dll");
                CopyFile("Windows/System32/D3DCOMPILER_47.dll");
                CopyFile("Windows/System32/dxcore.dll");
                CopyFile("Windows/System32/d3d10warp.dll");
                CopyFile("Windows/System32/directxdatabasehelper.dll");
                CopyFile("Windows/System32/ResourcePolicyClient.dll");
                CopyFile("Windows/System32/gameinput.dll");
                CopyFile("Windows/System32/windows.applicationmodel.dll");
                CopyFile("Windows/System32/d3d11.dll");
                CopyFile("Windows/System32/WindowManagement.dll");
                CopyFile("Windows/System32/windowmanagementapi.dll");
                CopyFile("Windows/System32/wuceffects.dll");
                CopyFile("Windows/apppatch/DirectXApps.sdb");
                CopyFile("Windows/apppatch/DirectXApps_FOD.sdb");
                CopyFile("Windows/System32/Windows.gaming.input.dll");
                CopyFile("Windows/System32/DispBroker.Desktop.dll");
                CopyFile("Windows/System32/DispBrokerDesktop.dll");
                CopyFile("Windows/System32/DispBroker.dll");
                CopyFile("Windows/System32/GameInputRedist.dll");
                CopyFile("Windows/System32/dwmghost.dll");
                CopyFile("Windows/System32/InputHost.dll");
                CopyFile("Windows/System32/Windows.Graphics.dll");
                CopyFile("Windows/System32/OneCoreUAPCommonProxyStub.dll");
                CopyFile("Windows/System32/UIAutomationCore.dll");
                CopyFile("Windows/System32/UIAnimation.dll");
                CopyKey(HiveTypes.Software, "Microsoft\\Windows\\Dwm");
                CopyKey(HiveTypes.Software, "Microsoft\\SecurityManager");
                CopyKey(HiveTypes.Software, "Microsoft\\WindowsRuntime");

             //   File.Copy(SourcePath + "Windows/system32/cmd.exe", Base + "Windows/system32/dwm.exe", true);
              //  File.Copy(SourcePath + "Windows/system32/dwm.exe", Base + "Windows/system32/dwm2.exe", true);
             //   File.Copy(SourcePath + "Windows/system32/en-us/dwm.exe.mui", Base + "Windows/system32/en-us/dwm2.exe.mui", true);

            }
            if (Builder.Options.UseModernTaskmgr)
            {
                CopyFile("Windows/System32/en-us/Taskmgr.exe.mui");
                CopyFile("Windows/System32/Taskmgr.exe");

                CopyFile("Windows/System32/LaunchTM.exe");
            }

            Builder.ReportProgress(false, 0, "Copying Required Registry");
            CopyKey(HiveTypes.Software, "Classes");
            CopyKey(HiveTypes.Software, "Microsoft\\Windows\\CurrentVersion\\Explorer");
            CopyKey(HiveTypes.Software, "Microsoft\\Windows NT\\CurrentVersion\\Svchost");
            CopyKey(HiveTypes.System, "ControlSet001\\Control\\ProductOptions");

            //add various tools
            Directory.CreateDirectory(Base + "tools");

            if (File.Exists("ProcMon64.exe"))
                File.Copy("ProcMon64.exe", Base + "tools/procmon.exe", true);

            TakeOwnership(Base + "windows/system32/startnet.cmd", false);
            File.WriteAllText(Base + "windows/system32/startnet.cmd", "@echo off\necho welcome!");

            SoftwareHive.SaveAndUnload();
            SystemHive.SaveAndUnload();

            InstallSoftwareHive.SaveAndUnload();
            InstallSystemHive.SaveAndUnload();
            return true;
            
        }
        private bool FixBlackScreen()
        {
            //From Win10XPE
            Process takeOwnProcess = new();
            ProcessStartInfo takeOwnStartInfo = new()
            {
                FileName = "SetACL.exe",

                // Do not write error output to standard stream.
                RedirectStandardError = true,
                // Do not write output to Process.StandardOutput Stream.
                RedirectStandardOutput = true,
                // Do not read input from Process.StandardInput (i/e; the keyboard).
                RedirectStandardInput = false,

                UseShellExecute = false,
                // Do not show a command window.
                CreateNoWindow = true,

                Arguments = $"-ot file -on \"{Base.Replace(@"\",@"\\")}\" -actn ace -actn setprot -op \"dacl:p_nc\" -ace n:S-1-1-0;p:full -silent"
            };
            Builder.ReportProgress(false, 0, "Taking ownership of files. Setting owner. This might take some time");
            takeOwnProcess.EnableRaisingEvents = true;
            takeOwnProcess.StartInfo = takeOwnStartInfo;
            takeOwnProcess.OutputDataReceived += (sender, e) => Debug.WriteLine(e.Data);

            // Start the process.
            takeOwnProcess.Start();
            takeOwnProcess.BeginOutputReadLine();
            takeOwnProcess.BeginErrorReadLine();
            // Wait for the process to exit.
            takeOwnProcess.WaitForExit();

            int exitCode = takeOwnProcess.ExitCode;
            bool takeOwnSuccessful = true;

            // Now we need to see if the process was successful.
            if (exitCode > 0 & !takeOwnProcess.HasExited)
            {
                takeOwnProcess.Kill();
                takeOwnSuccessful = false;
            }

            // Now clean up after ourselves.
            takeOwnProcess.Dispose();

            return takeOwnSuccessful;
        }
        private bool RunSetACL(string hive)
        {
            Process takeOwnProcess = new();
            ProcessStartInfo takeOwnStartInfo = new()
            {
                FileName = "SetACL.exe",

                // Do not write error output to standard stream.
                RedirectStandardError = true,
                // Do not write output to Process.StandardOutput Stream.
                RedirectStandardOutput = true,
                // Do not read input from Process.StandardInput (i/e; the keyboard).
                RedirectStandardInput = false,

                UseShellExecute = false,
                // Do not show a command window.
                CreateNoWindow = true,

                Arguments = $"-on HKLM\\{hive} -ot reg  -rec yes -actn setowner -ownr n:S-1-1-0 -silent"
            };
            Builder.ReportProgress(false, 0, "Taking ownership of registry (hive " + hive + "). Setting owner. This might take some time");
            takeOwnProcess.EnableRaisingEvents = true;
            takeOwnProcess.StartInfo = takeOwnStartInfo;
            takeOwnProcess.OutputDataReceived += (sender, e) => Debug.WriteLine(e.Data);

            // Start the process.
            takeOwnProcess.Start();
            takeOwnProcess.BeginOutputReadLine();
            takeOwnProcess.BeginErrorReadLine();
            // Wait for the process to exit.
            takeOwnProcess.WaitForExit();

            int exitCode = takeOwnProcess.ExitCode;
            bool takeOwnSuccessful = true;

            // Now we need to see if the process was successful.
            if (exitCode > 0 & !takeOwnProcess.HasExited)
            {
                takeOwnProcess.Kill();
                takeOwnSuccessful = false;
            }

            // Now clean up after ourselves.
            takeOwnProcess.Dispose();

            if (!takeOwnSuccessful)
                throw new Exception("Taking ownership of hive " + hive + "failed");
            Builder.ReportProgress(false, 50, "Taking ownership of registry (hive " + hive + "). Setting ACL. This might take some time");
            takeOwnProcess = new();
            takeOwnStartInfo = new()
            {
                FileName = "SetACL.exe",

                // Do not write error output to standard stream.
                RedirectStandardError = true,
                // Do not write output to Process.StandardOutput Stream.
                RedirectStandardOutput = true,
                // Do not read input from Process.StandardInput (i/e; the keyboard).
                RedirectStandardInput = false,

                UseShellExecute = false,
                // Do not show a command window.
                CreateNoWindow = true,

                Arguments = $"-on HKLM\\{hive} -ot reg -rec yes -actn ace -ace n:S-1-1-0;p:full -silent"
            };

            takeOwnProcess.EnableRaisingEvents = true;
            takeOwnProcess.StartInfo = takeOwnStartInfo;
            takeOwnProcess.OutputDataReceived += (sender, e) => Debug.WriteLine(e.Data);

            // Start the process.
            takeOwnProcess.Start();
            takeOwnProcess.BeginOutputReadLine();
            takeOwnProcess.BeginErrorReadLine();
            // Wait for the process to exit.
            takeOwnProcess.WaitForExit();

            exitCode = takeOwnProcess.ExitCode;
            takeOwnSuccessful = true;

            // Now we need to see if the process was successful.
            if (exitCode > 0 & !takeOwnProcess.HasExited)
            {
                takeOwnProcess.Kill();
                takeOwnSuccessful = false;
            }

            // Now clean up after ourselves.
            takeOwnProcess.Dispose();

            if (!takeOwnSuccessful)
                throw new Exception("Taking ownership of hive " + hive + "failed");
            Builder.ReportProgress(false, 50, "Completed take ownership of registry (hive " + hive + ")");
            return takeOwnSuccessful;
        }
        private void TakeOwnership(string path, bool recursive)
        {
            _ = TakeOwnership2(path, recursive);
            _ = GrantFullControl(path, "Administrators", recursive);
            _ = GrantFullControl(path, "SYSTEM", recursive);
            _ = GrantFullControl(path, "Everyone", recursive);
        }
        public static bool TakeOwnership2(string fileName, bool recursive)
        {
            Process takeOwnProcess = new();
            ProcessStartInfo takeOwnStartInfo = new()
            {
                FileName = "takeown.exe",

                // Do not write error output to standard stream.
                RedirectStandardError = true,
                // Do not write output to Process.StandardOutput Stream.
                RedirectStandardOutput = true,
                // Do not read input from Process.StandardInput (i/e; the keyboard).
                RedirectStandardInput = false,

                UseShellExecute = false,
                // Do not show a command window.
                CreateNoWindow = true,

                Arguments = "/f " + fileName + " /a "
            };

            if (recursive)
                takeOwnStartInfo.Arguments += "/r";

            takeOwnProcess.EnableRaisingEvents = true;
            takeOwnProcess.StartInfo = takeOwnStartInfo;
            takeOwnProcess.OutputDataReceived += (sender, e) => Debug.WriteLine(e.Data);

            // Start the process.
            takeOwnProcess.Start();
            takeOwnProcess.BeginOutputReadLine();
            takeOwnProcess.BeginErrorReadLine();
            // Wait for the process to exit.
            takeOwnProcess.WaitForExit();

            int exitCode = takeOwnProcess.ExitCode;
            bool takeOwnSuccessful = true;

            // Now we need to see if the process was successful.
            if (exitCode > 0 & !takeOwnProcess.HasExited)
            {
                takeOwnProcess.Kill();
                takeOwnSuccessful = false;
            }

            // Now clean up after ourselves.
            takeOwnProcess.Dispose();

            if (!takeOwnSuccessful)
                Debugger.Break();

            return takeOwnSuccessful;
        }
        public static bool GrantFullControl(string fileName, string userName, bool recursive)
        {
            Process grantFullControlProcess = new();
            ProcessStartInfo grantFullControlStartInfo = new()
            {
                FileName = "icacls.exe",

                // Do not write error output to standard stream.
                RedirectStandardError = true,
                // Do not write output to Process.StandardOutput Stream.
                RedirectStandardOutput = true,
                // Do not read input from Process.StandardInput (i/e; the keyboard).
                RedirectStandardInput = false,

                UseShellExecute = false,
                // Do not show a command window.
                CreateNoWindow = true,

                Arguments = fileName + " /grant " + userName + ":(F) "
            };
            if (recursive) grantFullControlStartInfo.Arguments += "/T";

            grantFullControlProcess.EnableRaisingEvents = true;
            grantFullControlProcess.StartInfo = grantFullControlStartInfo;
            grantFullControlProcess.OutputDataReceived += (sender, e) => Debug.WriteLine(e.Data);

            //   Logger.WriteLine("Running process: icacls " + grantFullControlStartInfo.Arguments + "\n");

            // Start the process.
            grantFullControlProcess.Start();
            grantFullControlProcess.BeginOutputReadLine();
            grantFullControlProcess.BeginErrorReadLine();
            grantFullControlProcess.WaitForExit();

            int exitCode = grantFullControlProcess.ExitCode;
            bool grantFullControlSuccessful = true;

            // Now we need to see if the process was successful.
            if (exitCode > 0 & !grantFullControlProcess.HasExited)
            {
                grantFullControlProcess.Kill();
                grantFullControlSuccessful = false;
            }

            // Now clean up after ourselves.
            grantFullControlProcess.Dispose();
            return grantFullControlSuccessful;
        }
        #region Registry takeown
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint RegOpenKeyEx(IntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, ref IntPtr phkResult);
        public const int HKEY_CURRENT_USER = unchecked((int)0x80000001);
        public const int HKEY_CLASSES_ROOT = unchecked((int)0x80000000);
        public const int KEY_WOW64_64KEY = 0x0100;
        public const int WRITE_OWNER = 0x00080000;
        public const int WRITE_DAC = 0x00040000;
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint RegCloseKey(IntPtr hKey);
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LookupPrivilegeValue([In] string lpSystemName, [In] string lpName, [Out] out LUID Luid);
        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public int LowPart;
            public int HighPart;
        }
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, IntPtr NewState, int BufferLength, IntPtr PreviousState, ref int ReturnLength);
        [StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public int Attributes;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_PRIVILEGES
        {
            internal int PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            internal int[] Privileges;
        }
        public const int TOKEN_ASSIGN_PRIMARY = 0x1;
        public const int TOKEN_DUPLICATE = 0x2;
        public const int TOKEN_IMPERSONATE = 0x4;
        public const int TOKEN_QUERY = 0x8;
        public const int TOKEN_QUERY_SOURCE = 0x10;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        public const int TOKEN_ADJUST_GROUPS = 0x40;
        public const int TOKEN_ADJUST_DEFAULT = 0x80;
        public const int TOKEN_ALL_ACCESS = TOKEN_ASSIGN_PRIMARY
              + TOKEN_DUPLICATE + TOKEN_IMPERSONATE + TOKEN_QUERY
              + TOKEN_QUERY_SOURCE + TOKEN_ADJUST_PRIVILEGES
              + TOKEN_ADJUST_GROUPS + TOKEN_ADJUST_DEFAULT;
        public const string SE_RESTORE_NAME = "SeRestorePrivilege";
        public const string SE_DEBUG_NAME = "SeDebugPrivilege";
        public const string SE_TCB_NAME = "SeTcbPrivilege";
        public const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
        public const int SE_PRIVILEGE_ENABLED_BY_DEFAULT = (0x00000001);
        public const int SE_PRIVILEGE_ENABLED = (0x00000002);
        public const int SE_PRIVILEGE_REMOVED = (0X00000004);
        public const int SE_PRIVILEGE_USED_FOR_ACCESS = unchecked((int)0x80000000);
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool OpenProcessToken(IntPtr hProcess, uint desiredAccess, out IntPtr hToken);
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint SetSecurityInfo(IntPtr handle, SE_OBJECT_TYPE ObjectType, uint SecurityInfo,
            IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl);
        public const int OWNER_SECURITY_INFORMATION = 0x00000001;
        public enum SE_OBJECT_TYPE
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
            SE_REGISTRY_WOW64_32KEY,
            SE_REGISTRY_WOW64_64KEY,
        }
        [Flags]
        internal enum SECURITY_INFORMATION : uint
        {
            OWNER_SECURITY_INFORMATION = 0x00000001,
            GROUP_SECURITY_INFORMATION = 0x00000002,
            DACL_SECURITY_INFORMATION = 0x00000004,
            SACL_SECURITY_INFORMATION = 0x00000008,
            UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
            UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
            PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
            PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000
        }
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool ConvertStringSidToSid(string StringSid, out IntPtr Sid);

        [DllImport("Advapi32.dll", SetLastError = true)]
        internal static extern Int32 RegSetKeySecurity(
        IntPtr hKey,
        SECURITY_INFORMATION SecurityInformation,
        ref SECURITY_DESCRIPTOR pSecurityDescriptor);
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
  string StringSecurityDescriptor,
  uint StringSDRevision,
  out SECURITY_DESCRIPTOR SecurityDescriptor,
  out ulong SecurityDescriptorSize
);
        #endregion
    }
}
