using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.RedBook.Models;
using HamsterStudio.RedBook.Models.Sub;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;
using System.Runtime.InteropServices;

namespace HamsterStudio.RedBook.Services;

public class NoteDownloadService(FileMgmt fileMgmt, CommonDownloader downloader)
{
    private readonly Logger _logger = Logger.Shared;

    public event Action<NoteDetailModel> OnNoteDetailUpdated = delegate { };

    public async Task<ServerRespModel> DownloadNoteAsync(NoteDataModel noteData)
    {
        var currentNote = noteData.NoteDetailMap[noteData.CurrentNoteId];
        var noteDetail = currentNote.NoteDetail;

        _logger.Information($"开始处理<{NoteDetailHelper.GetTypeName(noteDetail)}>作品：{noteData.CurrentNoteId}");
        return await DownloadNoteLowAsync(noteDetail,
            new NoteDataOptionsModel { WithComments = false, AuthorCommentsOnly = true },
            []);
    }

    public async Task<ServerRespModel> DownloadNoteLowAsync(NoteDetailModel noteDetail, NoteDataOptionsModel options, CommentModel[] comments)
    {
        OnNoteDetailUpdated?.Invoke(noteDetail);

        bool isHot = fileMgmt.AlbumCollections.AddAlbum(new AlbumCollectionModel
        {
            OwnerName = noteDetail.UserInfo.Nickname,
            FileCount = noteDetail.ImageList.Sum(x => x.LivePhoto ? 2 : 1) + (noteDetail.Type == "video" ? 1 : 0) + comments.Length,
            Albums = [noteDetail.Title]
        });

        if (isHot)
        {
            var indepent = fileMgmt.CreateSubFolder(noteDetail.UserInfo.Nickname);
            foreach (var file in indepent.Parent.GetFiles($"*_xhs_{noteDetail.UserInfo.Nickname}_*"))
            {
                string newName = Path.Combine(indepent.FullName, file.Name);
                File.Move(file.FullName, newName, true);
            }
        }

        var containedFiles = new List<string>();
        _logger.Information($"标题：{noteDetail.Title}【{noteDetail.ImageList.Count}】");

        var processor = new NoteDetailProcessor(noteDetail, fileMgmt, downloader, _logger);

        // 处理图片下载
        await processor.ProcessImageDownloads(containedFiles, isHot);

        // 处理视频下载
        if (noteDetail.Type == "video")
        {
            await processor.ProcessVideoDownload(containedFiles, isHot);
        }

        foreach (var comment in comments)
        {
            await processor.ProcessComment(comment, isHot);
        }

        _logger.Information("Done.");

        return BuildResponse(noteDetail, containedFiles);
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
                StaticFiles = [.. files.Select(f => $"xiaohongshu/{f}").OrderBy(f => f, comparer)]
            }
        };
    }

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string psz1, string psz2);

    private class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }

}

internal static class NoteDetailHelper
{
    public static string SelectTitle(NoteDetailModel noteDetail) =>
!string.IsNullOrEmpty(noteDetail.Title) ? noteDetail.Title : noteDetail.Time.ToString();

    public static string GetTypeName(NoteDetailModel noteDetail) =>
        noteDetail.Type switch { "normal" => "图文", "video" => "视频", _ => "Unknown" };

    public static StreamInfoModel SelectStream(StreamModel stream) =>
        stream.H266?.FirstOrDefault() ?? stream.H265?.FirstOrDefault() ??
        stream.H264?.FirstOrDefault() ?? stream.Av1?.FirstOrDefault() ??
        throw new ArgumentException("No valid stream");

    public static Uri GeneratePngLink(string token)
    {
        return new Uri($"https://ci.xiaohongshu.com/{token}?imageView2/format/png");
    }

    public static Uri GenerateWebpLink(string token)
    {
        return new Uri($"https://sns-img-bd.xhscdn.com/{token}");
    }

    public static Uri GenerateVideoLink(string token)
    {
        return new Uri($"https://sns-video-bd.xhscdn.com/{token}");
    }

    public static string ExtractToken(string url)
    {
        string token = url.Split("!").First();
        token = token.Substring(token.IndexOf5th('/') + 1);
        return token;
    }
}

internal class NoteDetailProcessor(NoteDetailModel noteDetail, FileMgmt fileMgmt, CommonDownloader downloader, Logger _logger)
{
    public async Task ProcessComment(CommentModel comment, bool isHot)
    {
        try
        {
            string title = NoteDetailHelper.SelectTitle(noteDetail);
            for (int index = 0; index < comment.ImageList.Length; index++)
            {
                string url = comment.ImageList[index];
                string token = "comment/" + url.Split('!').First().Split('/').Last();
                bool isAuthor = isPostAuthor(comment.Author, noteDetail.UserInfo.Nickname);
                var filename = fileMgmt.GenerateCommentImageFilename(title,
                    isAuthor ? noteDetail.UserInfo.Nickname : comment.Author,
                    comment.Id, index,
                    noteDetail.UserInfo, token,
                    isHot);

                var pnkLink = NoteDetailHelper.GeneratePngLink(token);
                if (!await downloader.EasyDownloadFileAsync(pnkLink, filename.FullName + ".png"))
                {
                    var webpLink = NoteDetailHelper.GenerateWebpLink(token);
                    if (!await downloader.EasyDownloadFileAsync(pnkLink, filename.FullName + ".webp"))
                    {
                        await downloader.EasyDownloadFileAsync(new Uri(url), filename.FullName + ".jpeg");
                        _logger.Error($"评论图片下载失败：{url}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Trace(ex);
            throw;
        }

        static bool isPostAuthor(string author, string postAuthor)
        {
            string[] parts = author.Split('\n').ToArray();
            return parts[0] == postAuthor;
        }
    }

    public async Task ProcessImageDownloads(List<string> containedFiles, bool isHot)
    {
        string title = NoteDetailHelper.SelectTitle(noteDetail);
        await Parallel.ForEachAsync(
            noteDetail.ImageList,
            new ParallelOptions { MaxDegreeOfParallelism = 6 },
            async (x, ct) => await ProcessImage(x));
        async Task ProcessImage(ImageListItemModel imgInfo)
        {
            int index = noteDetail.ImageList.IndexOf(imgInfo) + 1;

            // 生成文件名
            string token = NoteDetailHelper.ExtractToken(imgInfo.DefaultUrl);
            var fileInfo = fileMgmt.GenerateImageFilename(title, index, noteDetail.UserInfo, token, isHot);

            if (noteDetail.ImageList.IndexOf(imgInfo) == 0)
            {
                Logger.Shared.Information($"文件将会下载到{fileInfo.Directory}");
            }

            var filename = fileInfo.Name;

            // 检查PNG文件是否已存在
            string png_filename = filename + ".png";
            string png_full_filename = fileInfo.FullName + ".png";
            if (File.Exists(png_full_filename))
            {
                containedFiles.Add(png_filename);
                return;
            }

            // 检查WEBP文件是否已存在
            string webp_filename = filename + ".webp";
            string webp_full_filename = fileInfo.FullName + ".webp";
            if (File.Exists(webp_full_filename))
            {
                containedFiles.Add(webp_filename);
                return;
            }

            // 下载流程
            try
            {
                var pngUrl = NoteDetailHelper.GeneratePngLink(token);
                var resu = await downloader.EasyDownloadFileAsync(pngUrl, png_full_filename);
                if (resu)
                {
                    containedFiles.Add(png_filename);
                    //_logger.Information($"{png_full_filename}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
                }
                else
                {
                    var webpUrl = NoteDetailHelper.GenerateWebpLink(token);
                    resu = await downloader.EasyDownloadFileAsync(webpUrl, webp_full_filename);
                    if (resu)
                    {
                        containedFiles.Add(webp_filename);
                        //_logger.Information($"{webp_full_filename}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
                    }
                    else
                    {
                        _logger.Error($"下载失败：{imgInfo.DefaultUrl}【{imgInfo.Width}, {imgInfo.Height}】");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex.Message);
                var webpUrl = NoteDetailHelper.GenerateWebpLink(token);
                var resu = await downloader.EasyDownloadFileAsync(webpUrl, webp_full_filename);
                if (resu)
                {
                    containedFiles.Add(webp_filename);
                    //_logger.Information($"{webp_full_filename}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
                }
                else
                {
                    _logger.Error($"下载失败：{imgInfo.DefaultUrl}【{imgInfo.Width}, {imgInfo.Height}】");
                }
            }

            // 处理LivePhoto
            if (imgInfo.LivePhoto)
            {
                await ProcessLivePhoto(title, index, noteDetail.UserInfo, imgInfo, containedFiles, isHot);
            }

        }
    }

    private async Task<bool> ProcessLivePhoto(string title, int index, UserInfoModel user, ImageListItemModel imgInfo, List<string> containedFiles, bool isHot)
    {
        var streamInfo = NoteDetailHelper.SelectStream(imgInfo.Stream);
        var streamUrl = streamInfo.MasterUrl;
        var streamFile = fileMgmt.GenerateLivePhotoFilename(title, index, user, streamUrl, isHot);

        var streamFullPath = streamFile.FullName;
        var state = await downloader.EasyDownloadFileAsync(new Uri(streamUrl), streamFullPath);
        if (state)
        {
            containedFiles.Add(streamFile.Name);
            //_logger.Information($"LivePhoto {streamFile.Name} 下载成功。");
            return true;
        }

        _logger.Error($"LivePhoto 下载失败：{streamUrl}");
        return false;
    }

    public async Task ProcessVideoDownload(List<string> containedFiles, bool isHot)
    {
        string videoKey = noteDetail.VideoInfo.Consumer.OriginVideoKey;
        var videoUrl = NoteDetailHelper.GenerateVideoLink(videoKey);
        var videoFile = fileMgmt.GenerateVideoFilename(
            NoteDetailHelper.SelectTitle(noteDetail),
            noteDetail.UserInfo,
            videoKey.Split('/').Last(), isHot
        );

        string fullVideoPath = videoFile.FullName;
        var state = await downloader.EasyDownloadFileAsync(videoUrl, fullVideoPath, FileSizeDescriptor.FileSize_32M, true);
        if (state)
        {
            containedFiles.Add(videoFile.Name);
            _logger.Information($"视频 {videoFile.Name} 下载成功。");
        }
        else
        {
            _logger.Error($"视频下载失败：{videoUrl}");
        }
    }

}
