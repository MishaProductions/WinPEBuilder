using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public PEModder(Builder builder)
        {
            Base = builder.ImagePath;
            SourcePath = builder.SourcePath;
        }
        /// <summary>
        /// Copies a file to the image
        /// </summary>
        /// <param name="path">Windows/System32/shellstyle.dll</param>
        public void CopyFile(string path)
        {
            if (File.Exists(SourcePath + path))
            {
                File.Copy(SourcePath + path, Base + path, true);
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

            if(source == null || dest == null)
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

            var destKey = dest.RootKey.OpenSubKey(key);
            if (destKey == null)
            {
                destKey=dest.RootKey.CreateSubKey(key);
                if (destKey == null)
                {
                    Debug.WriteLine("failed to create key: " + key);
                    Debugger.Break();
                    return;
                }
            }

            CopyKeyInternal(sourceKey, destKey);
            Debugger.Break();
        }

        private void CopyKeyInternal(RegistryKey sourceKey, RegistryKey destKey)
        {
           
        }

        public void Run()
        {
            try
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

                //takeown config folder (required)
                TakeOwnership(SourcePath + "Windows/System32/config/", true);

                SoftwareHive = RegistyManager.MountHive(Base + "Windows/System32/config/SOFTWARE", "Tmp_Software");
                SystemHive = RegistyManager.MountHive(Base + "Windows/System32/config/System", "Tmp_System");

                InstallSoftwareHive = RegistyManager.MountHive(SourcePath + "Windows/System32/config/SOFTWARE", "Install_Software");
                InstallSystemHive = RegistyManager.MountHive(SourcePath + "Windows/System32/config/System", "Install_System");

                CopyKey(HiveTypes.Software, "Microsoft/Windows/Explorer");

                //add various tools
                Directory.CreateDirectory(Base + "tools");

                if (File.Exists("ProcMon64.exe"))
                    File.Copy("ProcMon64.exe", Base + "tools/procmon.exe");


                SoftwareHive.SaveAndUnload();
                SystemHive.SaveAndUnload();

                InstallSoftwareHive.SaveAndUnload();
                InstallSystemHive.SaveAndUnload();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
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
    }
}
