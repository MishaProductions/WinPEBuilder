using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core
{
    public static class RegistyManager
    {
        public static string GetTmpName()
        {
            return "WinPe_";
        }

        public static Hive MountHive(string path, string name)
        {
            Hive.AcquirePrivileges();
            Hive b = Hive.LoadFromFile(path, name);
            Console.WriteLine("Mounting Hive: " + name + " At " + b.RootKey);
            return b;
        }
    }
    public class Hive
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegLoadKey(IntPtr hKey, string lpSubKey, string lpFile);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegSaveKey(IntPtr hKey, string lpFile, uint securityAttrPtr = 0);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegUnLoadKey(IntPtr hKey, string lpSubKey);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern IntPtr RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);

        [DllImport("advapi32.dll")]
        static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref UInt64 lpLuid);

        [DllImport("advapi32.dll")]
        static extern bool LookupPrivilegeValue(IntPtr lpSystemName, string lpName, ref UInt64 lpLuid);

        private RegistryKey parentKey;
        private string name;
        private string originalPath;
        public RegistryKey RootKey { get; set; }

        private Hive() { }

        public static Hive LoadFromFile(string Path, string Name)
        {
            Hive result = new();

            result.parentKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            result.name = Name;//Guid.NewGuid().ToString();
            result.originalPath = Path;
            IntPtr parentHandle = result.parentKey.Handle.DangerousGetHandle();
            var x = RegLoadKey(parentHandle, result.name, Path);
            if (x != 0)
            {
                throw new Exception("RegLoadKey Failed: " + x);
            }
            //Console.WriteLine(Marshal.GetLastWin32Error());
            var key = result.parentKey.CreateSubKey(result.name, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (key == null)
            {
                throw new Exception("mounted key is null");
            }
            result.RootKey = key;
            return result;
        }
        public static void AcquirePrivileges()
        {
            ulong luid = 0;
            LookupPrivilegeValue(IntPtr.Zero, "SeRestorePrivilege", ref luid);
            RtlAdjustPrivilege((int)luid, true, false, out _);
            LookupPrivilegeValue(IntPtr.Zero, "SeBackupPrivilege", ref luid);
            RtlAdjustPrivilege((int)luid, true, false, out _);
        }
        public static void ReturnPrivileges()
        {
            ulong luid = 0;
            LookupPrivilegeValue(IntPtr.Zero, "SeRestorePrivilege", ref luid);
            RtlAdjustPrivilege((int)luid, false, false, out _);
            LookupPrivilegeValue(IntPtr.Zero, "SeBackupPrivilege", ref luid);
            RtlAdjustPrivilege((int)luid, false, false, out _);
        }
        public void SaveAndUnload()
        {
            RootKey.Close();
            int a = RegUnLoadKey(parentKey.Handle.DangerousGetHandle(), name);
            if (a != 0)
            {
                throw new Exception("RegUnloadKey Failed: " + a);
            }
            parentKey.Close();
        }
    }
    public enum HiveTypes { Software, System, Default }
}
