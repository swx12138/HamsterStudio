using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Download;
using HamsterStudio.Web.Strategies.StreamCopy;
using Microsoft.Extensions.Logging;
using HamstertFileInfo = HamsterStudio.Barefeet.FileSystem.HamstertFileInfo;

namespace HamsterStudio.Bilibili.Services.StreamDownloaders;

internal abstract class StreamDownloaderChaeine(StreamDownloaderChaeine? inner, ILogger? logger)
{
    protected ILogger? Logger => logger;
    protected FileStreamHttpContentCopyStrategy ContentCopyStrategy { get; } = new();
    protected FixedChunkSizeDownloadStrategy DownloadStrategy { get; } = new(FileSizeDescriptor.FileSize_32M, 3);

    public virtual async Task<DownloadStatus> Download(VideoStreamInfo streamInfo, AvMeta meta, HamstertFileInfo target)
    {
        if (inner != null)
        {
            return await inner.Download(streamInfo, meta, target);
        }
        logger?.LogWarning("No inner downloader provided, cannot download stream.");
        return DownloadStatus.Failed;
    }
}
