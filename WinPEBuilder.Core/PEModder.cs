using System;
using System.Collections.Generic;
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
            File.Copy(SourcePath + path, Base + path, true);
        }
        public void Run()
        {
            //needed
            CopyFile("Windows/System32/shellstyle.dll");

            //add various tools
            Directory.CreateDirectory(Base + "tools");

            if (File.Exists("ProcMon64.exe"))
                File.Copy("ProcMon64.exe", Base + "tools/procmon.exe");
        }
    }
}
