using DiscUtils;
using DiscUtils.Iso9660;
using DiscUtils.Udf;
using System.Diagnostics;
using System.Reflection.Emit;

namespace WinPEBuilder.Core
{
    public class Builder
    {
        public BuilderOptions Options { get; private set; }
        private readonly string IsoPath;
        public static string WorkingDir { get; set; } = "";
        public const string Version = "0.0.0.3a";
        public List<IPlugin> Plugins { get; }
        /// <summary>
        /// Location of the new image such as Z:\
        /// </summary>
        public string ImagePath { get; private set; } = "";
        /// <summary>
        /// Source path (eg: c:\mount\)
        /// </summary>
        public string SourcePath { get; private set; }

        /// <summary>
        /// Not thread safe
        /// </summary>
        public event BuilderEvent? OnProgress;
        /// <summary>
        /// Not thread safe
        /// </summary>
        public event EventHandler? OnComplete;
        /// <summary>
        /// Not thread safe - used to output to the log
        /// </summary>
        public event BuilderLogEvent? OnLog;
        /// <summary>
        /// Builder class
        /// </summary>
        /// <param name="options">Builder Options</param>
        /// <param name="isoPath">Path to ISO file</param>
        public Builder(BuilderOptions options, string isoPath, string WorkingDir)
        {
            //argument checking
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (string.IsNullOrEmpty(isoPath))
            {
                throw new ArgumentException($"'{nameof(isoPath)}' cannot be null or empty.", nameof(isoPath));
            }
            if (string.IsNullOrEmpty(WorkingDir))
            {
                throw new ArgumentException($"'{nameof(isoPath)}' cannot be null or empty.", nameof(isoPath));
            }

            //resolve plugins
            var plugins = PluginLoader.GetPlugins();
            Plugins = new List<IPlugin>();
            foreach (var guid in options.Plugins)
            {
                bool found = false;
                foreach (var plg in plugins)
                {
                    if (plg.PluginGuid == guid)
                    {
                        Plugins.Add(plg);
                        found = true;
                    }
                }
                if (!found)
                {
                    throw new Exception("Plugin with GUID " + guid + " cannot be found");
                }
            }

            this.Options = options;
            this.IsoPath = isoPath;
            Builder.WorkingDir = WorkingDir;
            SourcePath = WorkingDir + @"installwim\";
        }

        public void Start()
        {
            Thread t = new(WorkerThread);
            t.Start();
        }

        private void WorkerThread()
        {
            OnProgress?.Invoke(false, 0, "Setting up working directory");
            Directory.CreateDirectory(WorkingDir);
            Directory.CreateDirectory(WorkingDir + "installwim");
            Directory.CreateDirectory(WorkingDir + "iso");
            Directory.CreateDirectory(WorkingDir + "temp");
            ImagePath = @"Z:\";
            OnProgress?.Invoke(false, 0, "Extracting ISO");
            //1. Extract ISO (if needed)
            //2. Extract install.wim (if needed)
            //3. Create VHD
            //4. Apply boot.wim to the VHD
            //5. Do modifications
            //6. Close VHD, apply boot sector
            var d = Directory.GetDirectories(WorkingDir + @"iso\");
            if (d.Length == 0)
            {
                ExtractISO(IsoPath, WorkingDir + @"iso\");
            }
            OnProgress?.Invoke(false, 0, "Extracting install.wim");

            string installwim = WorkingDir + @"iso\sources\install.wim";
            string bootwim = WorkingDir + @"iso\sources\boot.wim";
            if (!File.Exists(installwim))
            {
                Cleanup(false);
                OnProgress?.Invoke(true, 0, "Invaild ISO: install.wim does not exist");
                return;
            }
            if (!File.Exists(bootwim))
            {
                Cleanup(false);
                OnProgress?.Invoke(true, 0, "Invaild ISO: boot.wim does not exist");
                return;
            }
            var d2 = Directory.GetDirectories(WorkingDir + @"installwim\");

            if (d2.Length == 0)
            {
                int exit = MountImage(installwim, 1, WorkingDir + @"installwim");

                if (exit != 0)
                {
                    Cleanup(false);
                    OnProgress?.Invoke(true, 0, "DISM exited with exit code " + exit + " while mounting install.wim");
                    return;
                }
            }
            OnProgress?.Invoke(false, 0, "Creating destination media");

            switch (Options.OutputType)
            {
                case BuilderOptionsOutputType.VHD:
                    var i = CreateVHD(Options.Output);

                    if (i != 0)
                    {
                        Cleanup(false);
                        OnProgress?.Invoke(true, 0, "Diskpart exited with exit code " + i + " while creating the VHD");
                        return;
                    }
                    ImagePath = @"Z:\";
                    OnProgress?.Invoke(false, 0, "Applying boot.wim to OS Image");
                    i = ApplyImage(bootwim, 1, ImagePath);
                    if (i != 0)
                    {
                        Cleanup(false);
                        OnProgress?.Invoke(true, 0, "Dism exited with exit code " + i + " while applying boot.wim to target");
                        return;
                    }
                    OnProgress?.Invoke(false, 0, "Installing boot sector");
                    i = VHDInstallBootSector();
                    if (i != 0)
                    {
                        Cleanup(false);
                        OnProgress?.Invoke(true, 0, "Bcdboot exited with exit code " + i + " while installing the boot sector to VHD");
                        return;
                    }
                    break;
                case BuilderOptionsOutputType.ISO:
                    throw new NotImplementedException("ISO output not yet supported");
                default:
                    break;
            }

            //Now that we have our image and everything ready, we can now mod it
            OnProgress?.Invoke(false, 0, "Modifing PE");
            var modder = new PEModder(this);
            var x = modder.Run();
            OnProgress?.Invoke(false, 0, "Unmount destination media");

            //We are done
            Cleanup(x);
        }
        private void Cleanup(bool normalExit)
        {
            OnProgress?.Invoke(false, 0, "Cleaning up");
            if (!normalExit)
                Debugger.Break();
            else
                OnComplete?.Invoke(this, new EventArgs());
            switch (Options.OutputType)
            {
                case BuilderOptionsOutputType.VHD:
                    //unmount VHD
                    int i = UnmountVHD(Options.Output);
                    if (i != 0 && normalExit)
                    {
                        OnProgress?.Invoke(true, 0, "diskpart exited with exit code " + i + " while unmounting the VHD. Please unmount it via Disk Manager");
                        return;
                    }
                    break;
                case BuilderOptionsOutputType.ISO:
                    throw new NotImplementedException("ISO output not yet supported");
                default:
                    break;
            }
        }
        private int VHDInstallBootSector()
        {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                FileName = "bcdboot",
                Arguments = $"{ImagePath}Windows /s x: /f ALL"
            };
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => Log(e.Data);
            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();
            var exit = process.ExitCode;
            process.CancelOutputRead();
            process.Close();
            return exit;
        }
        private int UnmountVHD(string isoPath)
        {
            string scriptfile = WorkingDir + @"temp\unmountvhd.txt";
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                FileName = "diskpart.exe",
                Arguments = $"/s \"{scriptfile}\""
            };
            process.StartInfo = startInfo;
            string script = "";

            //unmount vhd
            script += $"select vdisk file=\"{isoPath}\"\n";
            script += $"DETACH vdisk\n";

            File.WriteAllText(scriptfile, script);

            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => Log(e.Data);
            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();
            var exit = process.ExitCode;
            process.CancelOutputRead();
            process.Close();

            return exit;
        }
        private int CreateVHD(string isoPath)
        {
            string scriptfile = WorkingDir + @"temp\createvhd.txt";
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                FileName = "diskpart.exe",
                Arguments = $"/s \"{scriptfile}\""
            };
            process.StartInfo = startInfo;
            string script = "";
            if (File.Exists(isoPath))
            {

            }
            else
            {
                //create new vhd
                script += $"create vdisk file=\"{isoPath}\" maximum=4500\n";
            }

            //mount vhd
            script += $"select vdisk file=\"{isoPath}\"\n";
            script += $"attach vdisk\n";

            script += $"clean\n";

            //create main partition
            script += $"create partition primary size=3000\n";
            script += $"format fs=ntfs label=\"OSIMAGE\" quick\n";
            script += $"assign letter=z\n";
            script += $"active\n";

            //create boot partiton
            script += $"create partition primary\n";
            script += $"format fs=fat32 label=\"BOOT\" quick\n";
            script += $"assign letter=x\n";
            script += $"active\n";
            File.WriteAllText(scriptfile, script);

            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => Log(e.Data);
            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();
            var exit = process.ExitCode;
            process.CancelOutputRead();
            process.Close();

            return exit;
        }

        public int MountImage(string imageFile, int imageIndex, string mountdir)
        {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                FileName = "Dism.exe",
                Arguments = $"/Mount-image /imagefile:\"{imageFile}\" /index:{imageIndex} /MountDir:\"{mountdir}\""
            };
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => SetLabelText(e.Data, "Mounting install.wim");
            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();
            var exit = process.ExitCode;
            process.CancelOutputRead();
            process.Close();
            return exit;
        }
        public int ApplyImage(string imageFile, int imageIndex, string applydir)
        {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                FileName = "Dism.exe",
                Arguments = $"/Apply-image /imagefile:\"{imageFile}\" /index:{imageIndex} /applydir:{applydir}"
            };
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => SetLabelText(e.Data, "Applying " + Path.GetFileName(imageFile));
            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();
            var exit = process.ExitCode;
            process.CancelOutputRead();
            process.Close();
            return exit;
        }
        private void SetLabelText(string? text, string optxt)
        {
            try
            {
                Debug.WriteLine(text);
                if (text != null)
                {
                    if (text.Contains('%'))
                    {
                        var prg = text.Split('%')[0].Substring(text.Split('%')[0].Length - 4, 4);
                        decimal prgval = 0;
                        try
                        {
                            prgval = decimal.Parse(prg);
                        }
                        catch
                        {

                        }
                        OnProgress?.Invoke(false, (int)prgval, optxt + " " + prg + "% complete");
                    }
                    else if (text == "The operation completed successfully.")
                    {
                        OnProgress?.Invoke(false, 0, "Completed DISM operation");
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception) { }
        }
        void ExtractISO(string ISOName, string ExtractionPath)
        {
            try
            {
                using FileStream ISOStream = File.Open(ISOName, FileMode.Open);
                UdfReader Reader = new(ISOStream);

                ExtractDirectory(Reader.Root, ExtractionPath + "\\", "");
                Reader.Dispose();
            } catch (System.IO.IOException) { OnProgress?.Invoke(true, 0, "File is already in use by another process."); }
        }
        int prg = 0;
        void ExtractDirectory(DiscDirectoryInfo Dinfo, string RootPath, string PathinISO)
        {
            if (!string.IsNullOrWhiteSpace(PathinISO))
            {
                PathinISO += "\\" + Dinfo.Name;
            }
            RootPath += "\\" + Dinfo.Name;
            AppendDirectory(RootPath);
            foreach (DiscDirectoryInfo dinfo in Dinfo.GetDirectories())
            {
                ExtractDirectory(dinfo, RootPath, PathinISO);
            }
            foreach (DiscFileInfo finfo in Dinfo.GetFiles())
            {
                using Stream FileStr = finfo.OpenRead();
                using FileStream Fs = File.Create(RootPath + "\\" + finfo.Name); // Here you can Set the BufferSize Also e.g. File.Create(RootPath + "\\" + finfo.Name, 4 * 1024)
                FileStr.CopyTo(Fs, 16 * 1024); // Buffer Size is 16 * 1024 but you can modify it in your code as per your need
                if (prg >= 100)
                {
                    //todo improve progress
                    prg = 1;
                }
                OnProgress?.Invoke(false, prg++, "Extracting ISO");
            }
        }
        static void AppendDirectory(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (DirectoryNotFoundException)
            {
                var x = Path.GetDirectoryName(path);
                if (x != null)
                {
                    AppendDirectory(x);
                }
            }
            catch (PathTooLongException)
            {
                var x = Path.GetDirectoryName(path);
                if (x != null)
                {
                    AppendDirectory(x);
                }
            }
        }

        internal void ReportProgress(bool error, int prg, string message)
        {
            OnProgress?.Invoke(error, prg, message);
        }
        public void Log(string? message)
        {
            if (message != null)
            {
                Debug.WriteLine(message);
                OnLog?.Invoke(message + Environment.NewLine);
            }
        }
    }
}
