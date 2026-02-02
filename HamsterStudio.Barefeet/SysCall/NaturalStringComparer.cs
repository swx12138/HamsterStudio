namespace HamsterStudio.Barefeet.SysCall;

public class NaturalStringComparer : IComparer<string>
{
    public int Compare(string x, string y)
    {
        return ShellApi.StrCmpLogicalW(x, y);
    }
}