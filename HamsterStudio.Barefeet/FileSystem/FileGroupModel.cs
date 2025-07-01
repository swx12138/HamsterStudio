namespace HamsterStudio.Barefeet.FileSystem;

public partial class FileGroupModel(string groupName)
{
    public string GroupName { get; } = groupName;

    public HashSet<FileInfo> Files { get; } = [];

}
