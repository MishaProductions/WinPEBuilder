using DiscUtils;
using DiscUtils.Iso9660;
using DiscUtils.Udf;

namespace WinPEBuilder.Core
{
    public class Builder
    {
        private BuilderOptions Options;
        private string IsoPath;
        public static string WorkingDir = "";
        public static string Version = "0.0.0.1";

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