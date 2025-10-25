using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.RedBook.Models;
using HamsterStudio.RedBook.Models.Sub;
using HamsterStudio.Web.Services;

namespace HamsterStudio.RedBook.Services;

internal class NoteDetailProcessor(NoteDetailModel noteDetail, FileMgmt fileMgmt, CommonDownloader downloader, Logger _logger, bool isHot)
{
    public List<string> ContainedFiles { get; } = [];

    public async Task ProcessComment(CommentModel comment)
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
                var status = await downloader.EasyDownloadFileAsync(pnkLink, filename.FullName + ".png");
                if (DownloadStatus.Failed == status)
                {
                    var webpLink = NoteDetailHelper.GenerateWebpLink(token);
                    status = await downloader.EasyDownloadFileAsync(pnkLink, filename.FullName + ".webp");
                    if (DownloadStatus.Failed == status)
                    {
                        status = await downloader.EasyDownloadFileAsync(new Uri(url), filename.FullName + ".jpeg");
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

    public async Task ProcessImageDownloads()
    {
        string title = NoteDetailHelper.SelectTitle(noteDetail);
        await Parallel.ForEachAsync(
            noteDetail.ImageList,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 },
            async (x, ct) => await ProcessImage(x, title));
    }

    private async Task ProcessImage(ImageListItemModel imgInfo, string title)
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
            _logger.Information($"文件已存在：{png_filename}，跳过下载。");
            ContainedFiles.Add(png_filename);
            return;
        }

        // 检查WEBP文件是否已存在
        string webp_filename = filename + ".webp";
        string webp_full_filename = fileInfo.FullName + ".webp";
        if (File.Exists(webp_full_filename))
        {
            _logger.Information($"文件已存在：{webp_filename}，跳过下载。");
            ContainedFiles.Add(webp_filename);
            return;
        }

        // 下载流程
        try
        {
            var pngUrl = NoteDetailHelper.GeneratePngLink(token);
            var status = await downloader.EasyDownloadFileAsync(pngUrl, png_full_filename);
            if (status != DownloadStatus.Failed)
            {
                ContainedFiles.Add(png_filename);
            }
            else
            {
                var webpUrl = NoteDetailHelper.GenerateWebpLink(token);
                status = await downloader.EasyDownloadFileAsync(webpUrl, webp_full_filename);
                if (status != DownloadStatus.Failed)
                {
                    ContainedFiles.Add(webp_filename);
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
            var status = await downloader.EasyDownloadFileAsync(webpUrl, webp_full_filename);
            if (status != DownloadStatus.Failed)
            {
                ContainedFiles.Add(webp_filename);
            }
        }

        // 处理LivePhoto
        if (imgInfo.LivePhoto)
        {
            await ProcessLivePhoto(title, index, noteDetail.UserInfo, imgInfo);
        }

    }

    private async Task<bool> ProcessLivePhoto(string title, int index, UserInfoModel user, ImageListItemModel imgInfo)
    {
        var streamInfo = NoteDetailHelper.SelectStream(imgInfo.Stream);
        var streamUrl = streamInfo.MasterUrl;
        var streamFile = fileMgmt.GenerateLivePhotoFilename(title, index, user, streamUrl, isHot);

        var streamFullPath = streamFile.FullName;
        var status = await downloader.EasyDownloadFileAsync(new Uri(streamUrl), streamFullPath);
        if (status != DownloadStatus.Failed)
        {
            ContainedFiles.Add(streamFile.Name);
            return true;
        }

        _logger.Error($"LivePhoto 下载失败：{streamUrl}");
        return false;
    }

    public async Task ProcessVideoDownload()
    {
        string videoKey = noteDetail.VideoInfo.Consumer.OriginVideoKey;
        var videoUrl = NoteDetailHelper.GenerateVideoLink(videoKey);
        var videoFile = fileMgmt.GenerateVideoFilename(
            NoteDetailHelper.SelectTitle(noteDetail),
            noteDetail.UserInfo,
            videoKey.Split('/').Last(), isHot
        );

        string fullVideoPath = videoFile.FullName;
        var status = await downloader.EasyDownloadFileAsync(videoUrl, fullVideoPath, FileSizeDescriptor.FileSize_32M, true);
        if (status != DownloadStatus.Failed)
        {
            ContainedFiles.Add(videoFile.Name);
        }

    }

}
