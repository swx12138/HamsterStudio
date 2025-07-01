using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Models;
using FileInfo = HamsterStudio.Barefeet.FileSystem.FileInfo;

namespace HamsterStudio.Bilibili.Services.StreamDownloaders;

internal abstract class StreamDownloaderChaeine(StreamDownloaderChaeine? inner)
{
    public virtual async Task<bool> Download(VideoStreamInfo streamInfo, AvMeta meta, FileInfo target)
    {
        if (inner != null)
        {
            return await inner.Download(streamInfo, meta, target);
        }
        Logger.Shared.Warning("No inner downloader provided, cannot download stream.");
        return false;
    }
}

