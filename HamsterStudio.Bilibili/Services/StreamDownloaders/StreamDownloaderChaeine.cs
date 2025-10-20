using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Web.Strategies.Download;
using HamsterStudio.Web.Strategies.StreamCopy;
using HamstertFileInfo = HamsterStudio.Barefeet.FileSystem.HamstertFileInfo;

namespace HamsterStudio.Bilibili.Services.StreamDownloaders;

internal abstract class StreamDownloaderChaeine(StreamDownloaderChaeine? inner)
{
    protected DirectHttpContentCopyStrategy ContentCopyStrategy { get; } = new();
    protected FixedChunkSizeDownloadStrategy DownloadStrategy { get; } = new(20 * 1024 * 1024, Environment.ProcessorCount);

    public virtual async Task<bool> Download(VideoStreamInfo streamInfo, AvMeta meta, HamstertFileInfo target)
    {
        if (inner != null)
        {
            return await inner.Download(streamInfo, meta, target);
        }
        Logger.Shared.Warning("No inner downloader provided, cannot download stream.");
        return false;
    }
}

