using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Bilibili.Models.Sub;
using HamsterStudio.Bilibili.Services.Restful;
using HamsterStudio.Bilibili.Services.StreamDownloaders;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;

namespace HamsterStudio.Bilibili.Services;

public class BangumiDownloadService(
    IBilibiliApiService bilibiliApi,
    BiliApiClient blient,
    FileMgmt fileMgmt,
    CommonDownloader downloader,
    RequestStrategyProvider requestStrategyProvider)
{
    public string Cookies { get; set; } = string.Empty;

    public event Action<VideoInfo> OnVideoInfoUpdated = delegate { };

    private readonly StreamDownloaderChaeine _downloaderChain =
        new DashDownloader(downloader, fileMgmt, requestStrategyProvider.Strategy,
            new DurlDownloader(downloader, requestStrategyProvider.Strategy,
                null));

    public async Task<ServerRespModel> DownloadVideoByBvid(string bvid, int idx = -1)
    {
        try
        {
            if (bvid.IsNullOrEmpty())
            {
                return new ServerRespModel()
                {
                    Message = "Empty bvid!",
                    Status = -1,
                };
            }

            // 获取视频信息
            var videoInfoResp = await bilibiliApi.GetVideoInfoAsync(bvid);
            if (videoInfoResp.Code != 0)
            {
                return new ServerRespModel()
                {
                    Message = videoInfoResp.Message,
                    Status = (int)(0 - videoInfoResp.Code),
                };
            }

            var videoInfo = videoInfoResp.Data!;
            OnVideoInfoUpdated?.Invoke(videoInfo);

            idx = Math.Max(idx, 0);
            var target = fileMgmt.GetVideoFilename(videoInfo, idx);
            Logger.Shared.Information($"Output Dir:{target.FullName}");

            if (File.Exists(target.FullName))
            {
                Logger.Shared.Information($"{target.FullName} is already exists.");
            }
            else
            {
                Logger.Shared.Trace($"Downloading {bvid} to {target.FullName}");

                var page = videoInfo.Pages[idx];
                AvMeta meta = new()
                {
                    title = page.Title,
                    artist = videoInfo.Owner.Name!,
                    album = videoInfo.Title!,
                    copyright = videoInfo.Bvid!
                };

                //获取视频流信息
                //var videoStreamInfoResp = await bilibiliApi.GetVideoStreamInfoAsync(page.Cid, bvid, Cookies);
                // if (videoStreamInfoResp.Code != 0)
                // {
                //     return new ServerRespModel()
                //     {
                //         Message = videoStreamInfoResp.Message,
                //         Status = (int)(0 - videoStreamInfoResp.Code),
                //     };
                // }
                var videoStreamInfo = await blient.GetVideoStream(bvid, page.Cid) ?? throw new NotSupportedException(); // videoStreamInfoResp.Data;
                _ = await _downloaderChain.Download(videoStreamInfo, meta, target);
                _ = await SaveCover(videoInfo);
            }

            return new ServerRespModel()
            {
                Message = "Succeed",
                Status = 0,
                Data = new()
                {
                    Title = videoInfo.Title,
                    Description = videoInfo.Desc,
                    AuthorNickName = videoInfo.Owner.Name,
                    StaticFiles = []
                }
            };
        }
        catch (Exception ex)
        {
            Logger.Shared.Critical(ex);
            return new ServerRespModel()
            {
                Message = ex.Message,
                Status = -1
            };
        }
    }

    public async Task<string?> SaveCover(VideoInfo videoInfo)
    {
        return await SaveFile(videoInfo.Bvid, videoInfo.Pic);
    }

    public async Task<string?> SaveCover(WatchLaterDat watchLater)
    {
        return await SaveFile(watchLater.Bvid, watchLater.Pic);
    }

    public async Task<string?> SaveFirstFrame(string bvid, PagesItem pagesItem)
    {
        return await SaveFile(bvid, pagesItem.FirstFrame);
    }

    public async Task<string?> SaveOwnerFace(string bvid, Owner owner)
    {
        return await SaveFile(bvid, owner.Face);
    }

    public async Task<string?> SaveFile(string bvid, string url)
    {
        try
        {
            var dest = fileMgmt.GetCoverFilename(url, bvid);
            _ = await downloader.EasyDownloadFileAsync(new(url), dest.FullName);
            return dest.FullName;
        }
        catch (Exception ex)
        {
            Logger.Shared.Debug(ex);
            return null;
        }
    }

}
