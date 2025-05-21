using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Tools;
using System.Net;

namespace HamsterStudio.RedBook.Services.Download;

public class MediaDownloader(DownloadStrategyFactory? strategyFactory = null)
{
    private readonly DownloadStrategyFactory _strategyFactory = strategyFactory ?? new();

    public async Task<FileDownloadState> DownloadMediaAsync(string url, string savePath, bool isVideo = false)
    {
        var requ = new DownloadRequest(
            new Uri(url),
            isVideo ? GetVideoHeaders() : GetImageHeaders(),
            MaxConnections: isVideo ? 4 : 1);
        IDownloadStrategy strategy = _strategyFactory.CreateStrategy(requ.MaxConnections);

        try
        {
            var downloader = new Downloader(strategy);
            DownloadResult result = await downloader.ExecuteDownload(requ);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                await File.WriteAllBytesAsync(savePath, result.Data);
                return FileDownloadState.Succeed;
            }
        }
        catch (Exception ex)
        {
            Logger.Shared.Error($"下载异常：{url} | {ex.Message}");
            Logger.Shared.Debug(ex);
        }

        return FileDownloadState.Failed;
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
