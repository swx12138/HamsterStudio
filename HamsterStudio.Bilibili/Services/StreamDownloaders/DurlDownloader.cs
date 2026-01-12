using HamsterStudio.Bilibili.Models;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Request;
using Microsoft.Extensions.Logging;
using HamstertFileInfo = HamsterStudio.Barefeet.FileSystem.HamstertFileInfo;

namespace HamsterStudio.Bilibili.Services.StreamDownloaders;

internal class DurlDownloader(CommonDownloader downloader, AuthenticRequestStrategy strategy, StreamDownloaderChaeine? inner, ILogger? logger) : StreamDownloaderChaeine(inner, logger)
{
    public override async Task<DownloadStatus> Download(VideoStreamInfo videoStreamInfo, AvMeta meta, HamstertFileInfo target)
    {
        if (videoStreamInfo.Durl != null)
        {
            var status = await downloader.DownloadFileAsync(new(videoStreamInfo.Durl.First().Url), target.FullName, strategy, ContentCopyStrategy, DownloadStrategy);
            //if (status != DownloadStatus.Failed)
            {
                return status;
            }
        }
        return await base.Download(videoStreamInfo, meta, target);
    }
}

