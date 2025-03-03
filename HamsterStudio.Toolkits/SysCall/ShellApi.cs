using System.Runtime.InteropServices;

namespace HamsterStudio.Toolkits.SysCall
{
    public static class ShellApi
    {

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }
}
