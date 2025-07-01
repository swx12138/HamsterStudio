using HamsterStudio.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Services;

public interface IDirectoryMgmt
{
    string StorageHome { get; }
    string TemporaryHome { get; }
}

public class DirectoryMgmt : IDirectoryMgmt
{
    public string StorageHome { get; }
    public string TemporaryHome { get; } = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), 
        SystemConsts.ApplicationName);

    public DirectoryMgmt(string storageHome)
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
