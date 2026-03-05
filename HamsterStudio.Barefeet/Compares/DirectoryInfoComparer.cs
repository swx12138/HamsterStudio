using HamsterStudio.Barefeet.SysCall;

namespace HamsterStudio.Barefeet.Comparers;

public class DirectoryInfoComparer : IComparer<DirectoryInfo>
{
    int IComparer<DirectoryInfo>.Compare(DirectoryInfo? x, DirectoryInfo? y)
    {
        if (x == null) return -1;
        if (y == null) return 1;
        return ShellApi.StrCmpLogicalW(x.FullName, y.FullName);
    }
}
