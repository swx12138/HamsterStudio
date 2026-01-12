using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.SinaWeibo.Services;

public class FileMgmt
{
    public string Home { get; }

    internal UserNameMap UserNameMap { get; }

    //private Dictionary<string, FileInfo> FileInfos;

    public FileMgmt(DirectoryMgmt directoryMgmt, ILogger<FileMgmt> logger)
    {
        Home = Path.Combine(directoryMgmt.StorageHome, "weibo");
        if (!Directory.Exists(Home))
        {
            Directory.CreateDirectory(Home);
        }

        //FileInfos = Directory.GetFiles(Home, "*", new EnumerationOptions() { RecurseSubdirectories = true })
        //    .Select(x =>
        //    {
        //        var fileInfo = new FileInfo(x);
        //        return (fileInfo.Name, fileInfo);
        //    }).ToDictionary();
        UserNameMap = new(this);

        logger.LogInformation($"Weibo FileMgmt initialized, storage home: {Home}");
    }

    public string GetFullPath(string filename, string userId)
    {
        if (UserNameMap.CacheMap.TryGetValue(userId, out string? userName) && !userName.IsNullOrEmpty())
            return Path.Combine(Home, userName, filename);
        else
            return Path.Combine(Home, filename);
    }

    //public bool CheckFileExists(string filePath)
    //{
    //    var fileInfo = new FileInfo(filePath);
    //    return FileInfos.ContainsKey(fileInfo.Name) && FileInfos[fileInfo.Name].Exists;
    //}

}
