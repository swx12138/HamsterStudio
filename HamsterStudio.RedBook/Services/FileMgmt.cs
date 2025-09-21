using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.RedBook.Constants;
using HamsterStudio.RedBook.Models.Sub;
using HamsterStudio.RedBook.Services.Parsing;

namespace HamsterStudio.RedBook.Services;

public class FileMgmt : IDirectoryMgmt
{
    private readonly DirectoryMgmt _innerDirMgmt;

    public AlbumCollectionsModel AlbumCollections { get; }
    public string StorageHome { get; }
    public string TemporaryHome => _innerDirMgmt.TemporaryHome;
    public string CacheFilename { get; }

    public FileMgmt(DirectoryMgmt directoryMgmt)
    {
        _innerDirMgmt = directoryMgmt;
        StorageHome = Path.Combine(_innerDirMgmt.StorageHome, SystemConsts.HomeName);
        CacheFilename = Path.Combine(StorageHome, SystemConsts.CacheDirName, "album_collections.json");
        AlbumCollections = AlbumCollectionsModel.Load(CacheFilename, new FilenameInfoParser());
        CheckReallyHots();
        Logger.Shared.Information($"RedBook FileMgmt initialized, storage home: {StorageHome}");
    }

    private void CheckReallyHots()
    {
        Logger.Shared.Debug($"Running {nameof(FileMgmt)}::{nameof(CheckReallyHots)}() method.");
        foreach (var (albk, albs) in AlbumCollections.Collections)
        {
            if (albs.Albums.Count >= 10 || albs.FileCount > 36)
            {
                var indepent = CreateSubFolder(albs.OwnerName);
                foreach (var file in indepent.Parent.GetFiles($"*_xhs_{albs.OwnerName}_*"))
                {
                    string newName = Path.Combine(indepent.FullName, file.Name);
                    File.Move(file.FullName, newName);
                }
            }
        }
    }

    public DirectoryInfo CreateSubFolder(string subFolderName)
    {
        string subFolderPath = Path.Combine(StorageHome, subFolderName);
        if (!Directory.Exists(subFolderPath))
        {
            Directory.CreateDirectory(subFolderPath);
        }
        return new DirectoryInfo(subFolderPath);
    }

    private HamstertFileInfo GenerateFilename(UserInfoModel userInfo, string baseName, bool isHot)
    {
        if (isHot)
        {
            return new HamstertFileInfo(Path.Combine(StorageHome, userInfo.Nickname, $"{baseName}")) { RemoveCommand = null };
        }
        else
        {
            return new HamstertFileInfo(Path.Combine(StorageHome, $"{baseName}")) { RemoveCommand = null };
        }
    }

    public HamstertFileInfo GenerateImageFilename(string tiltle, int? index, UserInfoModel userInfo, string token, bool isHot)
    {
        string bareToken = token.Split('/').Last();
        string baseName = FileNameUtil.SanitizeFileName($"{tiltle}_{index}_xhs_{userInfo.Nickname}_{bareToken}");
        return GenerateFilename(userInfo, baseName, isHot);
    }

    public HamstertFileInfo GenerateLivePhotoFilename(string tiltle, int? index, UserInfoModel userInfo, string streamUrl, bool isHot)
    {
        var rawname = streamUrl.Split('?').First().Split('/').Last();
        string baseName = FileNameUtil.SanitizeFileName($"{tiltle}_{index}_xhs_{userInfo.Nickname}_{rawname}");
        return GenerateFilename(userInfo, baseName, isHot);
    }

    public HamstertFileInfo GenerateVideoFilename(string tiltle, UserInfoModel userInfo, string token, bool isHot)
    {
        // TBD:自动判断类型
        string baseName = FileNameUtil.SanitizeFileName($"{tiltle}_xhs_{userInfo.Nickname}_{token}.mp4");
        return GenerateFilename(userInfo, baseName, isHot);
    }

    public void Save()
    {
        AlbumCollections.Save(CacheFilename);
    }

}