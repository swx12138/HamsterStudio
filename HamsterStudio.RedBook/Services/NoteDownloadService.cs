using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.RedBook.Constants;
using HamsterStudio.RedBook.Models;
using HamsterStudio.RedBook.Models.Sub;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;

namespace HamsterStudio.RedBook.Services;

public class NoteDownloadService(DirectoryMgmt directoryMgmt, CommonDownloader downloader)
{
    public string StorageDirectory { get; set; } = Path.Combine(directoryMgmt.StorageHome, SystemConsts.HomeName);

    private readonly Logger _logger = Logger.Shared;

    public event Action<NoteDetailModel> OnNoteDetailUpdated = delegate { };

    public async Task<ServerRespModel> DownloadNoteAsync(NoteDataModel noteData)
    {
        var currentNote = noteData.NoteDetailMap[noteData.CurrentNoteId];
        var noteDetail = currentNote.NoteDetail;
        OnNoteDetailUpdated?.Invoke(noteDetail);

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
        await Parallel.ForEachAsync(
            noteDetail.ImageList,
            new ParallelOptions { MaxDegreeOfParallelism = 6 },
            async (x, ct) => await ProcessImage(x));
        async Task ProcessImage(ImageListItemModel imgInfo)
        {
            int index = noteDetail.ImageList.IndexOf(imgInfo) + 1;

            // 生成文件名
            string token = ExtractToken(imgInfo.DefaultUrl);
            var filename = FileNameGenerator.GenerateImageFilename(title, index, noteDetail.UserInfo, token);

            // 检查PNG文件是否已存在
            string png_filename = filename + ".png";
            string png_full_filename = GetFullPath(png_filename);
            if (File.Exists(png_full_filename))
            {
                _logger.Information($"文件已存在：{filename}，跳过下载。");
                containedFiles.Add(png_filename);
                return;
            }

            // 检查WEBP文件是否已存在
            string webp_filename = filename + ".webp";
            string webp_full_filename = GetFullPath(webp_filename);
            if (File.Exists(webp_full_filename))
            {
                _logger.Information($"文件已存在：{filename}，跳过下载。");
                containedFiles.Add(webp_filename);
                return;
            }

            // 下载流程
            try
            {
                var pngUrl = GeneratePngLink(token);
                var resu = await downloader.EasyDownloadFileAsync(pngUrl, png_full_filename);
                if (resu)
                {
                    containedFiles.Add(png_filename);
                    _logger.Information($"{png_full_filename}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
                }
                else
                {
                    var webpUrl = GenerateWebpLink(token);
                    resu = await downloader.EasyDownloadFileAsync(webpUrl, webp_full_filename);
                    if (resu)
                    {
                        containedFiles.Add(webp_filename);
                        _logger.Information($"{webp_full_filename}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
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
                var webpUrl = GenerateWebpLink(token);
                var resu = await downloader.EasyDownloadFileAsync(webpUrl, webp_full_filename);
                if (resu)
                {
                    containedFiles.Add(webp_filename);
                    _logger.Information($"{webp_full_filename}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
                }
                else
                {
                    _logger.Error($"下载失败：{imgInfo.DefaultUrl}【{imgInfo.Width}, {imgInfo.Height}】");
                }
            }


            // 处理LivePhoto
            if (imgInfo.LivePhoto)
            {
                await ProcessLivePhoto(title, index, noteDetail.UserInfo, imgInfo, containedFiles);
            }

        }
    }

    private async Task<bool> ProcessLivePhoto(string title, int index, UserInfoModel user, ImageListItemModel imgInfo, List<string> containedFiles)
    {
        var streamInfo = SelectStream(imgInfo.Stream);
        var streamUrl = streamInfo.MasterUrl;
        var streamFile = FileNameGenerator.GenerateLivePhotoFilename(title, index, user, streamUrl);

        var streamFullPath = GetFullPath(streamFile);
        var state = await downloader.EasyDownloadFileAsync(new Uri(streamUrl), streamFullPath, concurrent: true);
        if (state)
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
        var videoUrl = GenerateVideoLink(videoKey);
        string videoFile = FileNameGenerator.GenerateVideoFilename(
            SelectTitle(noteDetail),
            noteDetail.UserInfo,
            videoKey.Split('/').Last()
        );

        string fullVideoPath = GetFullPath(videoFile);
        var state = await downloader.EasyDownloadFileAsync(videoUrl, fullVideoPath, concurrent: true);
        if (state)
        {
            containedFiles.Add(videoFile);
            _logger.Information($"视频 {videoFile} 下载成功。");
        }
        else
        {
            _logger.Error($"视频下载失败：{videoUrl}");
        }
    }

    private ServerRespModel BuildResponse(NoteDetailModel noteDetail, List<string> files)
    {
        return new ServerRespModel
        {
            Message = "ok",
            Data = new ServerRespData
            {
                Title = noteDetail.Title,
                Description = noteDetail.Description,
                AuthorNickName = noteDetail.UserInfo.Nickname,
                StaticFiles = [.. files.Select(f => $"xiaohongshu/{f}")]
            }
        };
    }

    #region Helper Methods
    private string GetFullPath(string filename) => Path.Combine(StorageDirectory, filename);

    private async Task ApplyRandomDelay() =>
        await Task.Delay(100 * Random.Shared.Next(5));

    private string SelectTitle(NoteDetailModel noteDetail) =>
        !string.IsNullOrEmpty(noteDetail.Title) ? noteDetail.Title : noteDetail.Time.ToString();

    private static string GetTypeName(NoteDetailModel noteDetail) =>
        noteDetail.Type switch { "normal" => "图文", "video" => "视频", _ => "Unknown" };

    private StreamInfoModel SelectStream(StreamModel stream) =>
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

    private static string ExtractToken(string url)
    {
        string token = url.Split("!").First();
        token = token.Substring(token.IndexOf5th('/') + 1);
        return token;
    }
    #endregion
}