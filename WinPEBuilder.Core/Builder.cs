namespace WinPEBuilder.Core
{
    public class Builder
    {
        private BuilderOptions Options;
        private string IsoPath;
        public static string WorkingDir = "";
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
            Directory.CreateDirectory(WorkingDir + "output");
            Directory.CreateDirectory(WorkingDir + "temp");
        }
    }
}