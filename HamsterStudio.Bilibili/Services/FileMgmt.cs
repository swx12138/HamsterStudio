using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Bilibili.Constants;
using HamsterStudio.Bilibili.Models;
using System;
using FileInfo = HamsterStudio.Barefeet.FileSystem.FileInfo;

namespace HamsterStudio.Bilibili.Services;

public class FileMgmt : IDirectoryMgmt
{
    private readonly DirectoryMgmt _innerDirMgmt;
    private readonly HashSet<string> _subFolders;
    public string StorageHome { get; }
    public string TemporaryHome => _innerDirMgmt.TemporaryHome;
    public string DashHome { get; }
    public string CoverHome { get; }
    public string DynamicHome { get; }

    public FileMgmt(DirectoryMgmt directoryMgmt)
    {
        _innerDirMgmt = directoryMgmt;

        StorageHome = Path.Combine(_innerDirMgmt.StorageHome, SystemConsts.HomeName);
        
        DashHome = Path.Combine(StorageHome, SystemConsts.DashSubName);
        _subFolders = Directory.EnumerateDirectories(DashHome)
            .Select(x=>Path.GetFileName(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        CoverHome = Path.Combine(StorageHome, SystemConsts.CoverSubName);
        DynamicHome = Path.Combine(StorageHome, SystemConsts.DynamicSubName);
    }

    public FileInfo GetVideoFilename(VideoInfo videoInfo, int idx)
    {
        string filename = $"{videoInfo.Cid!}-{idx}_{videoInfo.Bvid}.mp4";     // TBD：修改命名规则，增加视频质量和音频质量
        string fullName = _subFolders.Contains(videoInfo.Owner.Name) ?
            Path.Combine(DashHome, videoInfo.Owner.Name, filename) :
            Path.Combine(DashHome, filename);    // TBD：分文件夹
        return new FileInfo(fullName);
    }

    public FileInfo GetCoverFilename(string url, string bvid)
    {
        string base_name = GetFilenameFromUrl(url);
        string filename = $"{bvid}_bili_{base_name}";        // tbd：将旧文件名重命名
        string fullName = Path.Combine(CoverHome, filename);    // TBD：分文件夹
        return new FileInfo(fullName);
    }

    public FileInfo GetDynamicFilename(string url, string dynamicId, int idx)
    {
        string base_name = GetFilenameFromUrl(url);
        string filename = $"{dynamicId}_{idx}_bili_{base_name}";
        string fullName = Path.Combine(CoverHome, filename);    // TBD：分文件夹
        return new FileInfo(fullName);
    }

    public static string GetFilenameFromUrl(string url) => url.Split("?")[0].Split("@")[0].Split('/').Where(x => !x.IsNullOrEmpty()).Last();

}
