namespace HamsterStudio.Barefeet.FileSystem;

public class FileManagerModel
{
    public HashSet<FileGroupModel> FileGroups { get; } = [];

    public int FileCount => FileGroups.Sum(x => x.Files.Count);

    public List<IFileManagerFilter> Filters { get; } = [];

    public IFileManagerGrouper Grouper { get; } = new DirFileManagerGrouper();

    public void AddFile(string filename)
    {
        if (!Filters.Any(x => x.Test(filename)))
        {
            return;
        }

        var groupName = Grouper.Group(filename);
        var group = FileGroups.FirstOrDefault(x => x.GroupName == groupName);
        if (group == null)
        {
            group = new FileGroupModel(groupName);
            FileGroups.Add(group);
        }
        group.Files.Add(new(filename));
    }

    public void ReadFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            AddFile(file);
        }
    }

}
