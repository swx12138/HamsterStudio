using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.RedBook.Constants;
using HamsterStudio.RedBook.Models;
using HamsterStudio.RedBook.Models.Sub;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.RedBook.Services;

public class FileMgmt : AbstractDirectoryMgmt
{
    private readonly DirectoryMgmt _innerDirMgmt;

    //public IHamsterMediaCollection AlbumCollections { get; }

    public override string StorageHome { get; }
    public override string TemporaryHome => _innerDirMgmt.TemporaryHome;
    //public string CacheFilename { get; }



    public FileMgmt(DirectoryMgmt directoryMgmt, ILogger<FileMgmt> logger) : base(logger)
    {
        _innerDirMgmt = directoryMgmt;
        StorageHome = Path.Combine(_innerDirMgmt.StorageHome, SystemConsts.HomeName);
        //CacheFilename = Path.Combine(StorageHome, SystemConsts.CacheDirName, "album_collections.json");
        //AlbumCollections = AlbumCollectionsModel.Load(CacheFilename, new FilenameInfoParser());

        {
            //var coll = new HamsterMediaCollectionModel(StorageHome, logger);
            //coll.Prepare();

            //string current_dir = Environment.CurrentDirectory;
            //Directory.SetCurrentDirectory(StorageHome);
            //coll.Enumerate((nickname, record) =>
            //{
            //    if (HamsterMediaCollectionModel.ShouldGroup(record))
            //    {
            //        //DoGroup(nickname);
            //        ShellApi.System($"xmd {nickname}");
            //    }
            //});
            //Directory.SetCurrentDirectory(current_dir);
            //AlbumCollections = coll;
        }
        //CheckReallyHots();

        logger.LogInformation($"RedBook FileMgmt initialized, storage home: {StorageHome}");
    }

    public void DoGroup(string nickname, out DirectoryInfo indepent)
    {
        logger?.LogTrace($"Grouping {nickname}");
        indepent = CreateSubFolder(nickname);
        foreach (var file in indepent.Parent.GetFiles($"*_xhs_{nickname}_*"))
        {
            string newName = Path.Combine(indepent.FullName, file.Name);
            File.Move(file.FullName, newName, true);
        }
    }

    private HamstertFileInfo GenerateFilename(DirectoryInfo home, string baseName, string? commentSubDir = null)
    {
        if (!string.IsNullOrEmpty(commentSubDir))
        {
            home = home.CreateSubdirectory(commentSubDir);
        }
        return new HamstertFileInfo(Path.Combine(home.FullName, $"{baseName}")) { RemoveCommand = null };
    }

    public HamstertFileInfo GenerateImageFilename(string tiltle, int index, UserInfoModel userInfo, string token, DirectoryInfo home)
    {
        return GenerateImageFilenameLow(tiltle, index, userInfo.Nickname, token, home);
    }

    public HamstertFileInfo GenerateImageFilenameLow(string tiltle, int index, string nickname, string token, DirectoryInfo home)
    {
        string bareToken = token.Split('/').Last();
        string baseName = FileNameUtil.SanitizeFileName($"{tiltle}_{index}_xhs_{nickname}_{bareToken}");
        return GenerateFilename(home, baseName);
    }

    public HamstertFileInfo GenerateCommentImageFilename(CommentDataModel comment, string title, int index, UserInfoModel userInfo, string token, string subDir, DirectoryInfo home)
    {
        string bareToken = token.Split('/').Last();
        string baseName = FileNameUtil.SanitizeFileName($"{title}_{comment.Id}_{index}_xhs_{comment.UserInfo.Nickname}_{bareToken}");
        return GenerateFilename(home, baseName, subDir);
    }

    public HamstertFileInfo GenerateLivePhotoFilename(string tiltle, int? index, UserInfoModel userInfo, string streamUrl, DirectoryInfo home)
    {
        var rawname = streamUrl.Split('?').First().Split('/').Last();
        string baseName = FileNameUtil.SanitizeFileName($"{tiltle}_{index}_xhs_{userInfo.Nickname}_{rawname}");
        return GenerateFilename(home, baseName);
    }

    public HamstertFileInfo GenerateVideoFilename(string tiltle, UserInfoModel userInfo, string token, DirectoryInfo home)
    {
        // TBD:自动判断类型
        string baseName = FileNameUtil.SanitizeFileName($"{tiltle}_xhs_{userInfo.Nickname}_{token}");
        if (!baseName.EndsWith(".mp4"))
        {
            baseName += ".mp4";
        }
        return GenerateFilename(home, baseName);
    }

    public void Save()
    {
        //AlbumCollections.Save(CacheFilename);
    }

}