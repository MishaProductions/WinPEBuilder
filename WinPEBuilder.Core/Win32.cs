using System.Runtime.InteropServices;

namespace WinPEBuilder.Core
{
    public class Win32
    {
        /// <summary>
        /// Enables required Win32 permissions
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static void Initialize()
        {
            if (!TokenManipulator.AddPrivilege("SeRestorePrivilege")) throw new Exception();
            if (!TokenManipulator.AddPrivilege("SeBackupPrivilege")) throw new Exception();
            if (!TokenManipulator.AddPrivilege("SeTakeOwnershipPrivilege")) throw new Exception();
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
        internal static extern uint RegCloseKey(IntPtr hKey);
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LookupPrivilegeValue([In] string lpSystemName, [In] string lpName, [Out] out LUID Luid);
        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID
        {
            public int LowPart;
            public int HighPart;
        }
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, IntPtr NewState, int BufferLength, IntPtr PreviousState, ref int ReturnLength);
        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID_AND_ATTRIBUTES
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
        internal const int TOKEN_ASSIGN_PRIMARY = 0x1;
        internal const int TOKEN_DUPLICATE = 0x2;
        internal const int TOKEN_IMPERSONATE = 0x4;
        internal const int TOKEN_QUERY = 0x8;
        internal const int TOKEN_QUERY_SOURCE = 0x10;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        internal const int TOKEN_ADJUST_GROUPS = 0x40;
        internal const int TOKEN_ADJUST_DEFAULT = 0x80;
        internal const int TOKEN_ALL_ACCESS = TOKEN_ASSIGN_PRIMARY
              + TOKEN_DUPLICATE + TOKEN_IMPERSONATE + TOKEN_QUERY
              + TOKEN_QUERY_SOURCE + TOKEN_ADJUST_PRIVILEGES
              + TOKEN_ADJUST_GROUPS + TOKEN_ADJUST_DEFAULT;
        internal const string SE_RESTORE_NAME = "SeRestorePrivilege";
        internal const string SE_DEBUG_NAME = "SeDebugPrivilege";
        internal const string SE_TCB_NAME = "SeTcbPrivilege";
        internal const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
        internal const int SE_PRIVILEGE_ENABLED_BY_DEFAULT = (0x00000001);
        internal const int SE_PRIVILEGE_ENABLED = (0x00000002);
        internal const int SE_PRIVILEGE_REMOVED = (0X00000004);
        internal const int SE_PRIVILEGE_USED_FOR_ACCESS = unchecked((int)0x80000000);
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool OpenProcessToken(IntPtr hProcess, uint desiredAccess, out IntPtr hToken);
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);
        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint SetSecurityInfo(IntPtr handle, SE_OBJECT_TYPE ObjectType, uint SecurityInfo,
            IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl);
        internal const int OWNER_SECURITY_INFORMATION = 0x00000001;
        internal enum SE_OBJECT_TYPE
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
        internal struct SECURITY_DESCRIPTOR
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