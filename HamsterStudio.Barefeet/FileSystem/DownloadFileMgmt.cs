using HamsterStudio.Barefeet.FileSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.FileSystem;

public class DownloadFileMgmt
{
    private IGroupManager GroupManager { get; }
    public string StorageHome { get; }

    public DownloadFileMgmt(string baseDir, IGroupManager groupManager)
    {
        GroupManager = groupManager;
        StorageHome = Path.GetFullPath(baseDir);
        if (!Directory.Exists(StorageHome))
        {
            Directory.CreateDirectory(StorageHome);
        }
    }

    public string CreateFile(string filename, string groupName)
    {
        string fullPath;
        if (GroupManager.CreateGroup(groupName))
        {
            string home = Path.Combine(StorageHome, groupName);
            if (!Directory.Exists(home))
            {
                Directory.CreateDirectory(home);
            }
            fullPath = Path.Combine(home, filename);
        }
        else
        {
            fullPath = Path.Combine(StorageHome, filename);
        }
        return fullPath;
    }
}
