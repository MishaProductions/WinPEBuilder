﻿using DiscUtils;
using DiscUtils.Iso9660;
using DiscUtils.Udf;
using System.Diagnostics;
using System.Reflection.Emit;

namespace WinPEBuilder.Core
{
    public class Builder
    {
        private BuilderOptions Options;
        private string IsoPath;
        public static string WorkingDir = "";
        public static string Version = "0.0.0.1";
        /// <summary>
        /// Location of the new image such as Z:\
        /// </summary>
        public string ImagePath { get; private set; } = "";
        /// <summary>
        /// Source path (eg: c:\mount\)
        /// </summary>
        public string SourcePath { get; private set; }

        /// <summary>
        /// Not thread safe!
        /// </summary>
        public event BuilderEvent? OnProgress;

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

            //check options
            if (!options.UseDWM)
            {
                if (options.EnableFullUWPSupport)
                {
                    throw new ArgumentException("Cannot install fullUWP support when DWM is not installed.", nameof(options));
                }
                if (options.UseLogonUI)
                {
                    throw new ArgumentException("Cannot install logonui when DWM is not installed.", nameof(options));
                }
            }

            this.Options = options;
            this.IsoPath = isoPath;
            Builder.WorkingDir = WorkingDir;
        }

        public void Start()
        {
            Thread t = new Thread(WorkerThread);
            t.Start();
        }
      
        private void WorkerThread()
        {
            OnProgress?.Invoke(false, 0, "Setting up working directory");
            Directory.CreateDirectory(WorkingDir);
            Directory.CreateDirectory(WorkingDir + "installwim");
            Directory.CreateDirectory(WorkingDir + "iso");
            Directory.CreateDirectory(WorkingDir + "temp");
         
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
            SourcePath = WorkingDir + @"installwim\";
            if (d2.Length == 0)
            {
                int exit = MountImage(installwim, 1, WorkingDir + @"installwim");
               
                if (exit != 0)
                {
                    Cleanup(false);
                    OnProgress?.Invoke(true, 0, "DISM exited with exit code "+exit+" while mounting install.wim");
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

            var modder = new PEModder(this);
            modder.Run();

            //We are done
            Cleanup(true);

        }
        private void Cleanup(bool normalExit)
        {
            if (!normalExit)
                Debugger.Break();
            switch (Options.OutputType)
            {
                case BuilderOptionsOutputType.VHD:
                    //unmount VHD
                    int i = UnmountVHD(Options.Output);
                    if (i != 0&&normalExit)
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
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                FileName = "bcdboot",
                Arguments = $"{ImagePath}Windows /s x: /f ALL"
            };
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => Debug.WriteLine(e.Data);
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
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
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
            process.OutputDataReceived += (sender, e) => Debug.WriteLine(e.Data);
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
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
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
                script += $"create vdisk file=\"{isoPath}\" maximum=2000\n";
            }

            //mount vhd
            script += $"select vdisk file=\"{isoPath}\"\n";
            script += $"attach vdisk\n";

            script += $"clean\n";

            //create main partition
            script += $"create partition primary size=1500\n";
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
            process.OutputDataReceived += (sender, e) => Debug.WriteLine(e.Data);
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
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                FileName = "Dism.exe",
                Arguments = $"/Mount-image /imagefile:\"{imageFile}\" /index:{imageIndex} /MountDir:\"{mountdir}\""
            };
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => setLabelText(e.Data, "Mounting install.wim");
            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();
            var exit=process.ExitCode;
            process.CancelOutputRead();
            process.Close();
            return exit;
        }
        public int ApplyImage(string imageFile, int imageIndex, string applydir)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                FileName = "Dism.exe",
                Arguments = $"/Apply-image /imagefile:\"{imageFile}\" /index:{imageIndex} /applydir:{applydir}"
            };
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.OutputDataReceived += (sender, e) => setLabelText(e.Data, "Applying "+Path.GetFileName(imageFile));
            process.Start();
            process.BeginOutputReadLine();

            process.WaitForExit();
            var exit = process.ExitCode;
            process.CancelOutputRead();
            process.Close();
            return exit;
        }
        private void setLabelText(string? text,string optxt)
        {
            try
            {
                Debug.WriteLine(text);
                if (text != null)
                {
                    if (text.Contains("%"))
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
                        OnProgress?.Invoke(false, (int)prgval, optxt +" "+ prg + "% complete");
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
            using (FileStream ISOStream = File.Open(ISOName, FileMode.Open))
            {
                UdfReader Reader = new UdfReader(ISOStream);

                ExtractDirectory(Reader.Root, ExtractionPath + "\\", "");
                Reader.Dispose();
            }
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
                using (Stream FileStr = finfo.OpenRead())
                {
                    using (FileStream Fs = File.Create(RootPath + "\\" + finfo.Name)) // Here you can Set the BufferSize Also e.g. File.Create(RootPath + "\\" + finfo.Name, 4 * 1024)
                    {
                        FileStr.CopyTo(Fs, 16 * 1024); // Buffer Size is 16 * 1024 but you can modify it in your code as per your need
                        if (prg >= 100)
                        {
                            //todo improve progress
                            prg = 1;
                        }
                        OnProgress?.Invoke(false, prg++, "Extracting ISO");
                    }
                }
            }
        }
        static void AppendDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (DirectoryNotFoundException Ex)
            {
                AppendDirectory(Path.GetDirectoryName(path));
            }
            catch (PathTooLongException Exx)
            {
                AppendDirectory(Path.GetDirectoryName(path));
            }
        }
    }
}