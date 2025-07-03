using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Models;
using HamstertFileInfo = HamsterStudio.Barefeet.FileSystem.HamstertFileInfo;

namespace HamsterStudio.Bilibili.Services.StreamDownloaders;

internal abstract class StreamDownloaderChaeine(StreamDownloaderChaeine? inner)
{
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

