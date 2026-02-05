using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.RedBook.Models;
using HamsterStudio.RedBook.Models.Sub;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.RedBook.Services;

public class NoteDownloadService(FileMgmt fileMgmt, DirectoryMgmt directoryMgmt, CommonDownloader downloader, Lazy<PreTokenCollector> tokenCollector, ILogger<NoteDownloadService> logger)
{
    public event Action<(NoteDetailModel, DirectoryInfo)> OnNoteDetailUpdated = delegate { };

    public event Action<string> OnImageTokenDetected = delegate { };

    public async Task<ServerRespModel> DownloadNoteAsync(NoteDataModel noteData)
    {
        var currentNote = noteData.NoteDetailMap[noteData.CurrentNoteId];
        var noteDetail = currentNote.NoteDetail;

        logger.LogInformation($"开始处理<{NoteDetailHelper.GetTypeName(noteDetail)}>作品：{noteData.CurrentNoteId}");
        return await DownloadNoteLowAsync(noteDetail, noteData.CurrentNoteId,
            new NoteDataOptionsModel { WithComments = false, AuthorCommentsOnly = true },
            new());
    }

    public bool IsHot(NoteDetailModel noteDetail, CommentsDataModel comments, bool downloadComments, out DirectoryInfo saveDirectory)
    {
        int imgCount = noteDetail.ImageList.Sum(x => x.LivePhoto ? 2 : 1) + (noteDetail.Type == "video" ? 1 : 0);
        bool isVeryHot = false;
        if (downloadComments)
        {
            int commentsPicCount = comments.Comments.Sum(x => x.Pictures.Length);
            if (commentsPicCount > 0)
            {
                isVeryHot = true;
            }
            imgCount += commentsPicCount;
        }
        bool isHot = fileMgmt.AlbumCollections.Update(noteDetail.UserInfo.Nickname, noteDetail.Title, imgCount) || isVeryHot;

        if (isHot)
        {
            string subfolder = FileNameUtil.SanitizeFileNameOr(noteDetail.UserInfo.Nickname, noteDetail.UserInfo.UserId);
            fileMgmt.DoGroup(subfolder, out saveDirectory);
        }
        else
        {
            saveDirectory = new DirectoryInfo(fileMgmt.StorageHome);
        }

        return isHot;
    }

    public async Task<ServerRespModel> DownloadNoteLowAsync(NoteDetailModel noteDetail, string noteId, NoteDataOptionsModel options, CommentsDataModel comments)
    {
        bool downloadComments = options.WithComments || options.AuthorCommentsOnly;
        bool isHot = IsHot(noteDetail, comments, downloadComments, out DirectoryInfo home);

        OnNoteDetailUpdated?.Invoke((noteDetail, home));
        logger.LogInformation($"标题：{noteDetail.Title}【{noteDetail.ImageList.Count}】");

        var processor = new NoteDetailProcessor(noteDetail, fileMgmt, downloader, logger, isHot);
        string title = NoteDetailHelper.SelectTitle(noteDetail);
        await Parallel.ForEachAsync(
            noteDetail.ImageList,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 },
            async (x, ct) => await processor.ProcessImage(x, title, OnImageTokenDetected, home));

        if (noteDetail.Type == "video")
        {
            await processor.ProcessVideoDownload(home);
        }

        // 下载评论区图片
        if (downloadComments)
        {
            foreach (var comment in comments.Comments)
            {
                await processor.ProcessComment(comment, title, options.AuthorCommentsOnly, noteId, home);
                _ = comment.SubComments
                    .Select(async x => await processor.ProcessComment(x, title, options.AuthorCommentsOnly, noteId, home))
                    .ToArray();
            }
        }

        logger.LogInformation("Done.");

        return BuildResponse(noteDetail, processor.ContainedFiles);
    }

    public async Task<ServerRespModel> DownloadWithBaseTokens(string[] tokens)
    {
        var processor = new NoteDetailProcessor(null, fileMgmt, downloader, logger, true);
        var preTokens = tokenCollector.Value.GetTokens();
        var downloadedFiles = new List<string>();
        foreach (var token in tokens)
        {
            var fileInfo = fileMgmt.GenerateImageFilenameLow("banned", 998, "-SpDownload", token, new DirectoryInfo(fileMgmt.StorageHome));
            if (!Directory.Exists(fileInfo.Directory))
            {
                Directory.CreateDirectory(fileInfo.Directory);
            }

            var status = await processor.DownloadImageByToken(token, fileInfo, true, shape: null);
            if (status != DownloadStatus.Failed)
            {
                downloadedFiles.Add(fileInfo.FullName);
            }
            else
            {
                foreach (var preToken in preTokens)
                {
                    status = await processor.DownloadImageByToken(preToken + token, fileInfo, false, shape: null);
                    if (status != DownloadStatus.Failed)
                    {
                        downloadedFiles.Add(fileInfo.FullName);
                    }
                }
            }
        }
        return new ServerRespModel
        {
            Message = "ok",
            Data = new ServerRespData
            {
                Title = "Base Tokens Download",
                Description = $"Downloaded {downloadedFiles.Count} files.",
                AuthorNickName = "-SpDownload",
                StaticFiles = downloadedFiles.ToArray()
            }
        };
    }

    private ServerRespModel BuildResponse(NoteDetailModel noteDetail, List<string> files)
    {
        var comparer = new NaturalStringComparer();
        return new ServerRespModel
        {
            Message = "ok",
            Data = new ServerRespData
            {
                Title = noteDetail.Title,
                Description = noteDetail.Description,
                AuthorNickName = noteDetail.UserInfo.Nickname,
                StaticFiles = [.. files
                    .Select(f => Path.GetRelativePath(directoryMgmt.StorageHome, f))
                    .Select(f=> f.Replace('\\','/'))
                    .OrderBy(f => f, comparer)]
            }
        };
    }

    public ServerRespModel DownloadComments(CommentsDataModel commentsData)
    {
        throw new NotImplementedException();
    }

}
