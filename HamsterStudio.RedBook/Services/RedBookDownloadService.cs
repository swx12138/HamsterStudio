using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.RedBook.DataModels;
using HamsterStudio.RedBook.Services.Download;
using System.IO;

namespace HamsterStudio.RedBook.Services;

public class RedBookDownloadService
{
    private readonly MediaDownloader _mediaDownloader;
    private readonly string _storageDir;
    private readonly Logger _logger = Logger.Shared;

    public RedBookDownloadService(
        MediaDownloader downloader,
        string storageDirectory)
    {
        _mediaDownloader = downloader;
        _storageDir = storageDirectory;
    }

    public async Task<ServerResp> DownloadNoteAsync(NoteDataModel noteData)
    {
        var currentNote = noteData.NoteDetailMap[noteData.CurrentNoteId];
        var noteDetail = currentNote.NoteDetail;
        var containedFiles = new List<string>();

        _logger.Information($"开始处理<{GetTypeName(noteDetail)}>作品：{noteData.CurrentNoteId}");
        _logger.Information($"标题：{noteDetail.Title}【{noteDetail.ImageList.Count}】");

        // 处理图片下载
        await ProcessImageDownloads(noteDetail, containedFiles);

        // 处理视频下载
        if (noteDetail.Type == "video")
        {
            await ProcessVideoDownload(noteDetail, containedFiles);
        }

        _logger.Information("Done.");

        return BuildResponse(noteDetail, containedFiles);
    }

    private async Task ProcessImageDownloads(NoteDetailModel noteDetail, List<string> containedFiles)
    {
        string title = SelectTitle(noteDetail);

        foreach (var imgInfo in noteDetail.ImageList)
        {
            int index = noteDetail.ImageList.IndexOf(imgInfo) + 1;
            bool shouldDelay = false;

            // 生成文件名
            var (pngFile, webpFile) = FileNameGenerator.GenerateImageFilenames(title, index, noteDetail.UserInfo, imgInfo);

            // 检查文件存在
            if (CheckExistingFiles(pngFile, webpFile, containedFiles)) continue;

            // 下载流程
            var downloadResult = await DownloadImageFiles(imgInfo, pngFile, webpFile);
            if (downloadResult != FileDownloadState.Failed)
            {
                containedFiles.Add(downloadResult == FileDownloadState.Succeed ? pngFile : webpFile);
                shouldDelay = true;
            }

            // 处理LivePhoto
            if (imgInfo.LivePhoto)
            {
                shouldDelay |= await ProcessLivePhoto(title, index, noteDetail.UserInfo, imgInfo, containedFiles);
            }

            if (shouldDelay)
            {
                await ApplyRandomDelay();
            }
        }
    }

    private async Task<FileDownloadState> DownloadImageFiles(ImageListItemModel imgInfo, string pngName, string webpName)
    {
        string pngUrl = GeneratePngLink(imgInfo.DefaultUrl);
        var state = await _mediaDownloader.DownloadMediaAsync(pngUrl, GetFullPath(pngName));

        if (state == FileDownloadState.Succeed)
        {
            _logger.Information($"{pngName}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
            return state;
        }

        string webpUrl = GenerateWebpLink(imgInfo.DefaultUrl);
        state = await _mediaDownloader.DownloadMediaAsync(webpUrl, GetFullPath(webpName));

        if (state == FileDownloadState.Succeed)
        {
            _logger.Information($"{webpName}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
            return FileDownloadState.Fallback;
        }

        _logger.Error($"下载失败：{imgInfo.DefaultUrl}");
        return FileDownloadState.Failed;
    }

    private async Task<bool> ProcessLivePhoto(string title, int index, UserInfoModel user, ImageListItemModel imgInfo, List<string> containedFiles)
    {
        var streamInfo = SelectStream(imgInfo.Stream);
        var streamUrl = streamInfo.MasterUrl;
        var streamFile = FileNameGenerator.GenerateLivePhotoFilename(title, index, user, streamUrl);

        var state = await _mediaDownloader.DownloadMediaAsync(streamUrl, GetFullPath(streamFile), isVideo: true);

        if (state == FileDownloadState.Succeed)
        {
            containedFiles.Add(streamFile);
            _logger.Information($"LivePhoto {streamFile} 下载成功。");
            return true;
        }

        _logger.Error($"LivePhoto 下载失败：{streamUrl}");
        return false;
    }

    private async Task ProcessVideoDownload(NoteDetailModel noteDetail, List<string> containedFiles)
    {
        string videoKey = noteDetail.VideoInfo.Consumer.OriginVideoKey;
        string videoUrl = GenerateVideoLink(videoKey);
        string videoFile = FileNameGenerator.GenerateVideoFilename(
            SelectTitle(noteDetail),
            noteDetail.UserInfo,
            videoKey.Split('/').Last()
        );

        var state = await _mediaDownloader.DownloadMediaAsync(videoUrl, GetFullPath(videoFile), isVideo: true);

        if (state == FileDownloadState.Succeed)
        {
            containedFiles.Add(videoFile);
            _logger.Information($"视频 {videoFile} 下载成功。");
        }
        else
        {
            _logger.Error($"视频下载失败：{videoUrl}");
        }
    }

    private ServerResp BuildResponse(NoteDetailModel noteDetail, List<string> files)
    {
        return new ServerResp
        {
            Message = "ok",
            Data = new ServerRespData
            {
                Title = noteDetail.Title,
                Description = noteDetail.Description,
                AuthorNickName = noteDetail.UserInfo.Nickname,
                StaticFiles = [.. files.Select(f => $"http://192.168.0.101:8899/static/xiaohongshu/{f}")]
            }
        };
    }

    #region Helper Methods
    private string GetFullPath(string filename) => Path.Combine(_storageDir, filename);

    private bool CheckExistingFiles(string pngFile, string webpFile, List<string> containedFiles)
    {
        if (File.Exists(GetFullPath(pngFile)))
        {
            containedFiles.Add(pngFile);
            _logger.Warning($"{pngFile}已存在。");
            return true;
        }

        if (File.Exists(GetFullPath(webpFile)))
        {
            containedFiles.Add(webpFile);
            _logger.Warning($"{webpFile}已存在。");
            return true;
        }

        return false;
    }

    private async Task ApplyRandomDelay() =>
        await Task.Delay(500 * Random.Shared.Next(5));

    private string SelectTitle(NoteDetailModel noteDetail) =>
        !string.IsNullOrEmpty(noteDetail.Title) ? noteDetail.Title : noteDetail.Time.ToString();

    private string GetTypeName(NoteDetailModel noteDetail) =>
        noteDetail.Type switch { "normal" => "图文", "video" => "视频", _ => "Unknown" };

    private StreamInfoModel SelectStream(StreamModel stream) =>
        stream.H266?.FirstOrDefault() ?? stream.H265?.FirstOrDefault() ??
        stream.H264?.FirstOrDefault() ?? stream.Av1?.FirstOrDefault() ??
        throw new ArgumentException("No valid stream");

    public string GeneratePngLink(string baseUrl)
    {
        return $"https://ci.xiaohongshu.com/{ExtractToken(baseUrl)}?imageView2/format/png";
    }

    public string GenerateWebpLink(string baseUrl)
    {
        return $"https://sns-img-bd.xhscdn.com/{ExtractToken(baseUrl)}";
    }

    public string GenerateVideoLink(string token)
    {
        //return $"https://sns-img-bd.xhscdn.com/{token}";
        return $"https://sns-video-bd.xhscdn.com/{token}";
    }

    private string ExtractToken(string url)
    {
        string token = url.Split("!").First();
        token = token.Substring(token.IndexOf5th('/') + 1);
        return token;
    }
    #endregion
}