using HamsterStudio.Barefeet;
using HamsterStudio.Barefeet.Constants;
using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.RedBook.Models;
using HamsterStudio.RedBook.Models.Sub;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Request;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.RedBook.Services;

public class NoteDetailProcessor(NoteDetailModel noteDetail, FileMgmt fileMgmt, CommonDownloader downloader, ILogger logger, bool isHot)
{
    public List<string> ContainedFiles { get; } = [];

    public async Task ProcessComment(CommentDataModel comment, string noteTitle, bool authorOnly, string noteId, DirectoryInfo home)
    {
        bool isAuthor = comment.ShowTags.Any(x => x == "is_author");
        if (authorOnly && !isAuthor)
        {
            return;
        }

        string title = NoteDetailHelper.SelectTitle(comment);
        foreach (var pic in comment.Pictures)
        {
            try
            {
                string url = pic.UrlDefault;
                string token = "comment/" + url.Split('!').First().Split('/').Last();
                int index = comment.Pictures.IndexOf(pic);
                var filename = fileMgmt.GenerateCommentImageFilename(comment, title, index, noteDetail.UserInfo, token, noteId, home);
                if (!Directory.Exists(filename.Directory))
                {
                    Directory.CreateDirectory(filename.Directory);
                }

                var shape = new MediaShape(pic.Width, pic.Height);
                var pnkLink = NoteDetailHelper.GeneratePngLink(token);
                var status = await downloader.EasyDownloadFileAsync(pnkLink, filename.FullName + ".png");
                if (DownloadStatus.Failed == status)
                {
                    var webpLink = NoteDetailHelper.GenerateWebpLink(token);
                    status = await downloader.EasyDownloadFileAsync(pnkLink, filename.FullName + ".webp");
                    if (DownloadStatus.Failed == status)
                    {
                        status = await downloader.EasyDownloadFileAsync(new Uri(url), filename.FullName + ".jpeg");
                        logger.LogError($"评论图片下载失败：{url}");
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogTrace(ex.Message + "\n" + ex.StackTrace);
                //throw;
            }
        }
    }

    public async Task<DownloadStatus> DownloadImageByToken(string token, HamstertFileInfo fileInfo, bool showDestPath, MediaShape? shape)
    {
        if (showDestPath)
        {
            logger.LogInformation($"文件将会下载到{fileInfo.Directory}");
        }

        var filename = fileInfo.Name;

        // 检查PNG文件是否已存在
        string png_filename = filename + ".png";
        string png_full_filename = fileInfo.FullName + ".png";
        if (File.Exists(png_full_filename))
        {
            logger.LogInformation($"文件已存在：{png_filename}，跳过下载。");
            ContainedFiles.Add(png_full_filename);
            return DownloadStatus.Exists;
        }

        // 检查WEBP文件是否已存在
        string webp_filename = filename + ".webp";
        string webp_full_filename = fileInfo.FullName + ".webp";
        if (File.Exists(webp_full_filename))
        {
            logger.LogInformation($"文件已存在：{webp_filename}，跳过下载。");
            ContainedFiles.Add(webp_full_filename);
            return DownloadStatus.Exists;
        }

        // 下载流程
        try
        {
            var pngUrl = NoteDetailHelper.GeneratePngLink(token);
            var status = await downloader.EasyDownloadFileAsync(pngUrl, png_full_filename, shape: shape);
            if (status != DownloadStatus.Failed)
            {
                ContainedFiles.Add(png_full_filename);
            }
            else
            {
                var webpUrl = NoteDetailHelper.GenerateWebpLink(token);
                status = await downloader.EasyDownloadFileAsync(webpUrl, webp_full_filename, shape: shape);
                if (status != DownloadStatus.Failed)
                {
                    ContainedFiles.Add(webp_full_filename);
                }

            }
            return status;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
            var webpUrl = NoteDetailHelper.GenerateWebpLink(token);
            var status = await downloader.EasyDownloadFileAsync(webpUrl, webp_full_filename);
            if (status != DownloadStatus.Failed)
            {
                ContainedFiles.Add(webp_full_filename);
            }
            return status;
        }
    }

    public async Task ProcessImage(ImageListItemModel imgInfo, string title, Action<string>? handleToken, DirectoryInfo home)
    {
        int index = noteDetail.ImageList.IndexOf(imgInfo) + 1;

        // 生成文件名
        string token = NoteDetailHelper.ExtractToken(imgInfo.DefaultUrl);
        handleToken?.Invoke(token);

        var fileInfo = fileMgmt.GenerateImageFilename(title, index, noteDetail.UserInfo, token, home);
        var shape = new MediaShape(imgInfo.Width, imgInfo.Height);
        var status = await DownloadImageByToken(token, fileInfo, noteDetail.ImageList.IndexOf(imgInfo) == 0, shape);
        if (status == DownloadStatus.Failed)
        {
            logger.LogError($"下载失败：{imgInfo.DefaultUrl}【{imgInfo.Width}, {imgInfo.Height}】");
        }

        // 处理LivePhoto
        if (imgInfo.LivePhoto)
        {
            await ProcessLivePhoto(title, index, noteDetail.UserInfo, imgInfo, home);
        }

    }

    private async Task<bool> ProcessLivePhoto(string title, int index, UserInfoModel user, ImageListItemModel imgInfo, DirectoryInfo home)
    {
        var streamInfo = NoteDetailHelper.SelectStream(imgInfo.Stream);
        var streamUrl = streamInfo.MasterUrl;
        var streamFile = fileMgmt.GenerateLivePhotoFilename(title, index, user, streamUrl, home);

        var streamFullPath = streamFile.FullName;
        var status = await downloader.EasyDownloadFileAsync(new Uri(streamUrl), streamFullPath);
        if (status != DownloadStatus.Failed)
        {
            ContainedFiles.Add(streamFile.FullName);
            return true;
        }

        logger.LogError($"LivePhoto 下载失败：{streamUrl}");
        return false;
    }

    private Lazy<AuthenticRequestStrategy> RequestStrategy = new(() =>
    {
        var handler = new HttpClientHandler();
        HttpClient client = new(handler);
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(BrowserConsts.EdgeUserAgent);
        client.DefaultRequestHeaders.Referrer = new Uri("https://www.xiaohongshu.com/");
        return new AuthenticRequestStrategy(client);
    });

    public Uri SelectVideoUrl(VideoInfoModel info)
    {
        string videoKey = info.Consumer.OriginVideoKey;
        if (!videoKey.IsNullOrEmpty())
        {
            return NoteDetailHelper.GenerateVideoLink(videoKey);
        }
        logger.LogWarning($"Not a valid key, key = \"{videoKey}\"");


        string result = string.Empty;
        if (GetUrlFromStream(info.Media.Stream.H266List, out result))
        {
            return new Uri(result);
        }
        if (GetUrlFromStream(info.Media.Stream.H265List, out result))
        {
            return new Uri(result);
        }
        if (GetUrlFromStream(info.Media.Stream.Av1List, out result))
        {
            return new Uri(result);
        }
        if (GetUrlFromStream(info.Media.Stream.H264List, out result))
        {
            return new Uri(result);
        }
        throw new ArgumentException("No valid video url found");

        static bool SelectUrlWithBackup(MediaStreamListItemModel item, out string url)
        {
            url = string.Empty;
            if (!item.MasterUrl.IsNullOrEmpty())
            {
                url = item.MasterUrl;
                return true;
            }
            if (item.BackupUrls != null && item.BackupUrls.Length > 0)
            {
                url = item.BackupUrls.Where(x => !x.IsNullOrEmpty()).First();
                return true;
            }
            return false;
        }

        static bool GetUrlFromStream(MediaStreamListItemModel[] items, out string url)
        {
            url = string.Empty;
            foreach (var item in items.OrderBy(x => x.VideoBitrate))
            {
                if (SelectUrlWithBackup(item, out url))
                {
                    return true;
                }
            }
            return false;
        }

    }

    public async Task ProcessVideoDownload(DirectoryInfo home)
    {
        var videoUrl = SelectVideoUrl(noteDetail.VideoInfo);
        logger.LogInformation("视频链接：" + videoUrl);

        var videoFile = fileMgmt.GenerateVideoFilename(
            NoteDetailHelper.SelectTitle(noteDetail),
            noteDetail.UserInfo,
            videoUrl.AbsolutePath.Split('?')[0].Split('/').Last(), 
            home
        );

        string fullVideoPath = videoFile.FullName;
        var status = await downloader.AuthenticatedDownloadFileAsync(videoUrl, fullVideoPath, RequestStrategy.Value, FileSizeDescriptor.FileSize_32M, true);
        if (status != DownloadStatus.Failed)
        {
            ContainedFiles.Add(videoFile.FullName);
        }

    }

}
