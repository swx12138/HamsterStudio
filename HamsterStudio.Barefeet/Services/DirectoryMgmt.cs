using HamsterStudio.Barefeet.MVVM;
using HamsterStudio.Constants;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.Barefeet.Services;

public interface IDirectoryMgmt
{
    string StorageHome { get; }
    string TemporaryHome { get; }

    DirectoryInfo CreateSubFolder(string subFolderName);
}

public abstract class AbstractDirectoryMgmt(ILogger? logger) : BindableBase(logger), IDirectoryMgmt
{
    public abstract string StorageHome { get; }
    public abstract string TemporaryHome { get; }

    public DirectoryInfo CreateSubFolder(string subFolderName)
    {
        string subFolderPath = Path.Combine(StorageHome, subFolderName);
        if (!Directory.Exists(subFolderPath))
        {
            Directory.CreateDirectory(subFolderPath);
        }
        return new DirectoryInfo(subFolderPath);
    }
}

public class DirectoryMgmt : AbstractDirectoryMgmt
{
    public override string StorageHome { get; }
    public override string TemporaryHome { get; } = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        SystemConsts.ApplicationName);

    public DirectoryMgmt(string storageHome, ILogger? logger) : base(logger)
    {
        StorageHome = storageHome;
        if (!Directory.Exists(StorageHome))
        {
            Directory.CreateDirectory(StorageHome);
        }
        if (!Directory.Exists(TemporaryHome))
        {
            Directory.CreateDirectory(TemporaryHome);
        }
    }
}
