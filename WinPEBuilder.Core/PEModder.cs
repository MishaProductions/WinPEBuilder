using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static WinPEBuilder.Core.PEModder;

namespace WinPEBuilder.Core
{
    /// <summary>
    /// This class actually modifies things
    /// </summary>
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
        internal static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
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
                    LogEvent("Error while copying file: " + ex.ToString());
                }

            }
            else
            {
                LogEvent("filenotfound: " + path);
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
                LogEvent("Warning: Source key is null: " + key);
                Debugger.Break();
                return;
            }

            var destKey = dest.RootKey.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (destKey == null)
            {
                destKey = dest.RootKey.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (destKey == null)
                {
                    LogEvent("Warning: Failed to create key: " + key);
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
                    LogEvent("Warning: Opensubkeyfailed: " + item);
                }
            }
        }
        public void CopyDir(string path)
        {
            if (Directory.Exists(SourcePath + path))
            {
                //check if dest exists
                if (Directory.Exists(Base + path))
                {
                    TakeOwnership(Base + path, true);
                }
                try
                {
                    CopyDirectory(SourcePath + path, Base + path);
                }
                catch (Exception ex)
                {
                    LogEvent("Error while copying file: " + ex.ToString());
                }

            }
            else
            {
                LogEvent("warning: Directory to copy not found: " + path);
            }
        }
        public void CopyService(string name)
        {
            try
            {
                CopyKey(HiveTypes.System, "ControlSet001\\Services\\" + name);
            }
            catch (Exception ex)
            {
                LogEvent("Warning: copy service error: " + ex.ToString());
            }
        }
        internal bool Run()
        {
            //needed for modern explorer
            CopyFile("Windows/System32/shellstyle.dll");
            // Partial implementation of BCH (only folder): Directory.CreateDirectory(Base + "Windows/BugCheckHack");
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
            CopyFile("Windows/System32/taskkill.exe");
            CopyFile("Windows/System32/en-us/taskkill.exe.mui");
            CopyFile("Windows/System32/thumbcache.dll");
            CopyFile("Windows/System32/en-us/explorerframe.dll.mui");
            CopyFile("Windows/SystemResources/imageres.dll.mun");
            CopyFile("Windows/SystemResources/explorerframe.dll.mun");
            CopyFile("Windows/fonts/segoeui.ttf");
            FixBlackScreen();
            //takeown config folder (required)
            TakeOwnership(SourcePath + "Windows/System32/config/", false);

            SoftwareHive = RegistyManager.MountHive(Base + "Windows/System32/config/SOFTWARE", "Tmp_Software");
            SystemHive = RegistyManager.MountHive(Base + "Windows/System32/config/System", "Tmp_System");

            InstallSoftwareHive = RegistyManager.MountHive(SourcePath + "Windows/System32/config/SOFTWARE", "Install_Software");
            InstallSystemHive = RegistyManager.MountHive(SourcePath + "Windows/System32/config/System", "Install_System");

            Builder.ReportProgress(false, 0, "Taking ownership of registry");
            Parallel.ForEach(new string[] { "Install_Software", "Install_System", "Tmp_Software", "Tmp_System" }, delegate (string x)
            {
                RunSetACL(x);
            });
            Builder.ReportProgress(false, 0, "Copying needed files");
            CopyService("TrustedInstaller");
            


            Builder.ReportProgress(false, 0, "Copying Required Registry");
            CopyKey(HiveTypes.Software, "Classes");
            CopyKey(HiveTypes.Software, "Microsoft\\RPC");
            CopyKey(HiveTypes.Software, "Microsoft\\Windows\\CurrentVersion\\Explorer");
            CopyKey(HiveTypes.Software, "Microsoft\\Windows\\CurrentVersion\\AppModel");
            CopyKey(HiveTypes.Software, "Microsoft\\Windows\\CurrentVersion\\Authentication");
            CopyKey(HiveTypes.Software, "Microsoft\\Windows NT\\CurrentVersion\\Winlogon");
            CopyKey(HiveTypes.Software, "Microsoft\\COM3");

            CopyKey(HiveTypes.Software, "Microsoft\\Windows\\CurrentVersion\\SharedPC");

            CopyKey(HiveTypes.Software, "Microsoft\\AppModel");
            CopyKey(HiveTypes.Software, "Microsoft\\Windows NT\\CurrentVersion\\Svchost");
            CopyKey(HiveTypes.Software, "Microsoft\\SQMClient");
            CopyKey(HiveTypes.System, "ControlSet001\\Control\\ProductOptions");
            CopyKey(HiveTypes.System, "ControlSet001\\Control\\FeatureManagement");
            //CopyKey(HiveTypes.System, "ControlSet001\\Policies\\Microsoft\\FeatureManagement");

            Builder.ReportProgress(false, 0, "Process plugins");
            int curPlugin = 0;
            foreach (var item in Builder.Plugins)
            {
                Builder.ReportProgress(false, (int)Math.Round((double)(100 * curPlugin) / Builder.Plugins.Count), "Processing plugin: " +item.DisplayName);
                item.Run(this);
                curPlugin++;
            }

            //add various tools
            Directory.CreateDirectory(Base + "tools");

            if (File.Exists("ProcMon64.exe"))
                File.Copy("ProcMon64.exe", Base + "tools/ProcMon64.exe", true);
            if (File.Exists("PENetwork.exe"))
                File.Copy("PENetwork.exe", Base + "tools/PENetwork.exe", true);
            if (File.Exists("PENetwork.ini"))
                File.Copy("PENetwork.ini", Base + "tools/PENetwork.ini", true);
            if (File.Exists("PENetwork_en-US.lng"))
                File.Copy("PENetwork_en-US.lng", Base + "tools/PENetwork_en-US.lng", true);
            Directory.CreateDirectory(Base + "tools/windbg/");
            if (Directory.Exists("windbg"))
                CopyDirectory("windbg", Base + "tools/windbg/");

            TakeOwnership(Base + "windows/system32/startnet.cmd", false);
            File.WriteAllText(Base + "windows/system32/startnet.cmd", "@echo off\necho welcome!");

            SoftwareHive.SaveAndUnload();
            SystemHive.SaveAndUnload();

            InstallSoftwareHive.SaveAndUnload();
            InstallSystemHive.SaveAndUnload();
            return true;

        }
        public static void CopyDirectory(string sourcepath, string targetpath)
        {
            var source = new DirectoryInfo(sourcepath);
            var target = new DirectoryInfo(targetpath);
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyDirectory(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }
        public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyDirectory(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
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

                Arguments = $"-ot file -on \"{Base.Replace(@"\", @"\\")}\" -actn ace -actn setprot -op \"dacl:p_nc\" -ace n:S-1-1-0;p:full -silent"
            };
            Builder.ReportProgress(false, 0, "Taking ownership of files. Setting owner. This might take some time");
            takeOwnProcess.EnableRaisingEvents = true;
            takeOwnProcess.StartInfo = takeOwnStartInfo;
            takeOwnProcess.OutputDataReceived += (sender, e) => LogEvent(e.Data);

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
            takeOwnProcess.OutputDataReceived += (sender, e) => LogEvent(e.Data);

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
            takeOwnProcess.OutputDataReceived += (sender, e) => LogEvent(e.Data);

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
        public void TakeOwnership(string path, bool recursive)
        {
            if (recursive)
            {
                _ = TakeOwnership2(path, recursive);
                _ = GrantFullControl(path, "Administrators", recursive);
                _ = GrantFullControl(path, "SYSTEM", recursive);
                _ = GrantFullControl(path, "Everyone", recursive);
            }
            else
            {
                var i = new FileInfo(path);
                var fs = i.GetAccessControl();
                bool ownerChanged = false;
                bool accessChanged = false;
                try
                {
                    fs.SetOwner(WindowsIdentity.GetCurrent().User);
                    i.SetAccessControl(fs); //Update the Access Control on the File
                    ownerChanged = true;
                }
                catch (PrivilegeNotHeldException) { }
                finally { }

                try
                {
                    fs.SetAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User, FileSystemRights.FullControl, AccessControlType.Allow));
                    i.SetAccessControl(fs);
                    accessChanged = true;
                }
                catch (UnauthorizedAccessException) { }
                Console.WriteLine("ownerchanged:" + ownerChanged + ",accesschanged:" + accessChanged);
            }

        }
        public bool TakeOwnership2(string fileName, bool recursive)
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
            takeOwnProcess.OutputDataReceived += (sender, e) => LogEvent(e.Data);

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
        public bool GrantFullControl(string fileName, string userName, bool recursive)
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
            grantFullControlProcess.OutputDataReceived += (sender, e) => LogEvent(e.Data);

            //Logger.WriteLine("Running process: icacls " + grantFullControlStartInfo.Arguments + "\n");

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
        /// <summary>
        /// Logs an event
        /// </summary>
        /// <param name="message">The message to log</param>
        public void LogEvent(string? message)
        {
            if (message != null)
            {
                Builder.Log(message);
            }
        }
    }
}
