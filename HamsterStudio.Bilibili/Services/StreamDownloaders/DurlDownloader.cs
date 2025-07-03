using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Tools;
using HamstertFileInfo = HamsterStudio.Barefeet.FileSystem.HamstertFileInfo;

namespace HamsterStudio.Bilibili.Services.StreamDownloaders;

internal class DurlDownloader(CommonDownloader downloader, AuthenticRequestStrategy strategy, StreamDownloaderChaeine? inner) : StreamDownloaderChaeine(inner)
{
    public override async Task<bool> Download(VideoStreamInfo videoStreamInfo, AvMeta meta, HamstertFileInfo target)
    {
        if (videoStreamInfo.Durl != null)
        {
            var dreq = new DownloadRequest(new(videoStreamInfo.Durl.First().Url), strategy);
            if (await downloader.DownloadFileAsync(dreq, target.FullName))
            {
                Logger.Shared.Information($"Durl下载成功！{target.FullName}");
            }
            else
            {
                Logger.Shared.Information($"Durl下载失败！");
            }
        }
        return await base.Download(videoStreamInfo, meta, target);
    }
}

