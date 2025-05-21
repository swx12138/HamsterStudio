using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels.ReadBook;
using HamsterStudio.Web.Tools;
using System.Net;

namespace HamsterStudio.Web.Utilities;

public class RedBookDownloader(string storageDir)
{
    private readonly DownloadStrategyFactory _strategyFactory = new();

    public async Task<ServerResp> Download(NoteDataModel noteData)
    {
        var currentNote = noteData.NoteDetailMap[noteData.CurrentNoteId];
        Logger.Shared.Information($"开始处理<{RedBookHelper.GetTypeName(currentNote.NoteDetail)}>作品：{noteData.CurrentNoteId}");
        Logger.Shared.Information($"标题：{currentNote.NoteDetail.Title}【{currentNote.NoteDetail.ImageList.Count}】");
        List<string> contained_files = [];
        string title = RedBookHelper.SelectTitle(currentNote.NoteDetail);
        foreach (var imgInfo in currentNote.NoteDetail.ImageList)
        {
            string token = imgInfo.DefaultUrl.Split("!").First().Split("/").Where(x => x != null && x.Length > 0).Last();
            int index = currentNote.NoteDetail.ImageList.IndexOf(imgInfo) + 1;

            //  检查是否存在PNG
            string png_filename = RedBookHelper.GenerateFilename(title, index, currentNote.NoteDetail.UserInfo, token);
            string pngFullPath = GetFullPath(png_filename);
            if (File.Exists(pngFullPath))
            {
                contained_files.Add(png_filename);
                Logger.Shared.Warning($"{png_filename}已存在。");
                continue;
            }

            // 检查是否存在WEBP
            string webp_filename = png_filename.Replace(".png", ".webp");
            string webpFullPath = GetFullPath(webp_filename);
            if (File.Exists(webpFullPath))
            {
                contained_files.Add(webp_filename);
                Logger.Shared.Warning($"{webp_filename}已存在。");
                continue;
            }

            bool shouldDelay = false;
            string png_url = RedBookHelper.GeneratePngLink(imgInfo.DefaultUrl);
            switch (await DownloadMedia(png_url, pngFullPath))
            {
                case FileDownlaodState.Succeed:
                    contained_files.Add(png_filename);
                    shouldDelay = true;
                    Logger.Shared.Information($"{png_filename}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
                    break;
                case FileDownlaodState.Failed:
                    {
                        string webp_url = RedBookHelper.GenerateWebpLink(imgInfo.DefaultUrl);
                        if (await DownloadMedia(webp_url, webpFullPath) == FileDownlaodState.Failed)
                        {
                            goto default;
                        }
                        else
                        {
                            contained_files.Add(webp_filename);
                            shouldDelay = true;
                            Logger.Shared.Information($"{webp_filename}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
                        }
                    }
                    break;
                default:
                    Logger.Shared.Error($"[{index}/{currentNote.NoteDetail.ImageList.Count}]下载失败。");
                    break;
            }

            if (imgInfo.LivePhoto)
            {
                var streamInfo = RedBookHelper.SelectStream(imgInfo.Stream);
                var streamUrl = streamInfo.MasterUrl;
                var streamFilename = RedBookHelper.GenerateLivePhotoFilename(title, index, currentNote.NoteDetail.UserInfo, streamUrl);
                var streamFullPath = GetFullPath(streamFilename);
                var result = await DownloadMedia(streamUrl, streamFullPath, true);
                if (result != FileDownlaodState.Failed)
                {
                    contained_files.Add(streamFilename);
                    shouldDelay = true;
                    Logger.Shared.Information($"LivePhoto{streamFilename}下载成功。");
                }
                else
                {
                    Logger.Shared.Error($"[{index}/{currentNote.NoteDetail.ImageList.Count}] LivePhoto下载失败。");
                }
            }
            if (shouldDelay)
            {
                await Task.Delay(500 * Random.Shared.Next(5));
            }
        }

        if ("video" == currentNote.NoteDetail.Type)
        {
            string video_url = RedBookHelper.GenerateVideoLink(currentNote.NoteDetail.VideoInfo.Consumer.OriginVideoKey);
            string filename = RedBookHelper.GenerateVideoFilename(title, currentNote.NoteDetail.UserInfo, currentNote.NoteDetail.VideoInfo.Consumer.OriginVideoKey.Split('/').Last());
            string videoFullPath = GetFullPath(filename);
            if (await DownloadMedia(video_url, videoFullPath, true) == FileDownlaodState.Succeed)
            {
                contained_files.Add(filename);
                Logger.Shared.Information($"视频{filename}下载成功。");
            }
            else
            {
                Logger.Shared.Error($"视频{filename}下载失败。");
            }
        }

        Logger.Shared.Information("Done.");

        return new ServerResp
        {
            Message = "ok",
            Data = new ServerRespData
            {
                Title = currentNote.NoteDetail.Title,
                Description = currentNote.NoteDetail.Description,
                AuthorNickName = currentNote.NoteDetail.UserInfo.Nickname,
                StaticFiles = [.. contained_files.Select(x => "http://192.168.0.101:8899/static/xiaohongshu/" + x)],
            }
        };
    }

    private string GetFullPath(string filename)
    {
        var fullPath = Path.Combine(storageDir, filename);
        return fullPath;
    }

    private async Task<FileDownlaodState> DownloadMedia(string url, string fullPath, bool isVideo = false)
    {
        var requ = new DownloadRequest(
            new Uri(url),
            isVideo ? GetVideoHeaders() : GetImageHeaders(),
            MaxConnections: isVideo ? 4 : 1);
        var strategy = _strategyFactory.CreateStrategy(requ.MaxConnections);

        try
        {
            var downloader = new Downloader(strategy);
            var result = await downloader.ExecuteDownload(requ);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                await File.WriteAllBytesAsync(fullPath, result.Data);
                return FileDownlaodState.Succeed;
            }
        }
        catch (Exception ex)
        {
            Logger.Shared.Error($"下载异常：{url} | {ex.Message}");
            Logger.Shared.Debug(ex);
        }

        return FileDownlaodState.Failed;
    }

    private static Dictionary<string, string> GetImageHeaders() => new()
    {
        ["accept"] = "image/webp,image/apng,image/*,*/*;q=0.8",
        ["referer"] = "https://www.xiaohongshu.com/"
    };

    private Dictionary<string, string> GetVideoHeaders()
    {
        return new()
        {
            ["accept"] = "video/mp4,video/webm,video/*",
            ["referer"] = "https://www.xiaohongshu.com/"
        };
    }
}
