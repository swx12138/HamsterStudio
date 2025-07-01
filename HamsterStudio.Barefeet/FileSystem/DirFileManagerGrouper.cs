namespace HamsterStudio.Barefeet.FileSystem;

public class DirFileManagerGrouper : IFileManagerGrouper
{
    public const string DefaultGroup = "Others";
    public string Group(string filename)
    {
        return Directory.GetParent(filename)?.FullName ?? DefaultGroup;
    }
}
