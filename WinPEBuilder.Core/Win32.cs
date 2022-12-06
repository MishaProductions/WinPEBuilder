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
    }
}