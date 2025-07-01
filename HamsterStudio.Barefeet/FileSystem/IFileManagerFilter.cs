namespace HamsterStudio.Barefeet.FileSystem;

public interface IFileManagerFilter
{
    bool Test(string filename);

}
