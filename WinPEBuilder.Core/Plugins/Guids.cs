using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core.Plugins
{
    /// <summary>
    /// Plugin GUID's used internally in WinPEBuilder.Core.dll
    /// </summary>
    public static class Guids
    {
        public static readonly Guid DWM = new Guid("59c0311a-0369-4a4e-82e8-66ac16585791");
        public static readonly Guid Explorer = new Guid("9b4fac74-aacc-418e-aeb7-ced0b2f07565");
        public static readonly Guid LogonUI = new Guid("87a6a9ce-0336-451d-ab74-a6085dd97a54");
        public static readonly Guid ModernTaskmgr = new Guid("c8d848f1-abba-418e-b18f-864143c214cb");
        public static readonly Guid FullUWPSupport = new Guid("8fb9200b-71ae-441e-a6d5-dcf2dd7ad722");
        public static readonly Guid MMCSupport = new Guid("67006eb3-8496-4244-ac8c-fffc188c5d6b");
        public static readonly Guid BSOD = new Guid("51bf25ac-6f18-4e21-92c3-8d96b80e04f8");
    }
}
