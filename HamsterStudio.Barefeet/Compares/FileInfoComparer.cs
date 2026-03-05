using HamsterStudio.Barefeet.SysCall;

namespace HamsterStudio.Barefeet.Comparers;

public class FileInfoComparer : IComparer<FileInfo>
{
    int IComparer<FileInfo>.Compare(FileInfo? x, FileInfo? y)
    {
        if (x == null) return -1;
        if (y == null) return 1;
        return ShellApi.StrCmpLogicalW(x.FullName, y.FullName);
    }
}
