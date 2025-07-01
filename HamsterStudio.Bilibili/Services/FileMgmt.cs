using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Bilibili.Constants;
using HamsterStudio.Bilibili.Models;
using FileInfo = HamsterStudio.Barefeet.FileSystem.FileInfo;

namespace HamsterStudio.Bilibili.Services;

public class FileMgmt : IDirectoryMgmt
{
    private readonly DirectoryMgmt _innerDirMgmt;

    public string StorageHome { get; }
    public string TemporaryHome => _innerDirMgmt.TemporaryHome;
    public string DashHome { get; }
    public string CoverHome { get; }

    public FileMgmt(DirectoryMgmt directoryMgmt)
    {
        _innerDirMgmt = directoryMgmt;

        StorageHome = Path.Combine(_innerDirMgmt.StorageHome, SystemConsts.HomeName);
        DashHome = Path.Combine(StorageHome, SystemConsts.DashSubName);
        CoverHome = Path.Combine(StorageHome, SystemConsts.CoverSubName);
    }

    public FileInfo GetVideoFilename(VideoInfo videoInfo, int idx)
    {
        string filename = $"{videoInfo.Cid!}-{idx}_{videoInfo.Bvid}.mp4";     // TBD：修改命名规则，增加视频质量和音频质量
        string fullPath = Path.Combine(DashHome, filename);    // TBD：分文件夹
        return new FileInfo(fullPath);
    }

    public static string GetFilenameFromUrl(string url) => url.Split("?")[0].Split("@")[0].Split('/').Where(x => !x.IsNullOrEmpty()).Last();

    public static string FormatImageFilename(string url, string bvid)
    {
        string filename = GetFilenameFromUrl(url);
        // tbd：将旧文件名重命名
        return $"{bvid}_bili_{filename}";
    }

    public static string FormatImageFilename(string url, string dynamicId, int idx)
    {
        string filename = GetFilenameFromUrl(url);
        return $"{dynamicId}_{idx}_bili_{filename}";
    }

}
