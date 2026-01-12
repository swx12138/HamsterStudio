using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Bilibili.Models.Sub;
using HamsterStudio.Bilibili.Services.Restful;
using HamsterStudio.Bilibili.Services.StreamDownloaders;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.Bilibili.Services;

public class BangumiDownloadService(
    ILogger<BangumiDownloadService> logger,
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
            new DurlDownloader(downloader, requestStrategyProvider.Strategy, null, logger), logger);

    public async Task DownloadReplies(string bvid)
    {
        int replayCount = 0;

        for (int page = 0; page < 10; page++)
        {
            var resp = await bilibiliApi.GetReplayV2(bvid, page, File.ReadAllText(fileMgmt.CookiesFile));
            if (resp.Code != 0)
            {
                logger.LogError("Load page failed.");
                return;
            }

            if (resp.Data.Replies == null)
            {
                continue;
            }

            foreach (var replay in resp.Data.Replies)
            {
                replayCount++;
                if (replay.Content.Pictures.Length <= 0)
                {
                    continue;
                }

                await DownloadRepliesImage(replay.Content.Pictures, replay.OidStr, bvid);
            }

            if (replayCount >= resp.Data.Page.Count)
            {
                break;
            }
        }

        logger.LogInformation($"{replayCount} replies.");
    }

    public async Task DownloadRepliesImage(ReplayPictureModel[] pictures, string replyId, string bvid)
    {
        foreach (var pic in pictures)
        {
            var filename = fileMgmt.GetReplayFilename(pic.ImageSrc, replyId, bvid, pictures.IndexOf(pic));
            if (!Directory.Exists(filename.Directory))
            {
                Directory.CreateDirectory(filename.Directory);
            }
            _ = await downloader.EasyDownloadFileAsync(new Uri(pic.ImageSrc), filename.FullName);
        }
    }

    public async Task<ServerRespModel> DownloadVideoByBvid(string bvid, int idx = -1, long cid = -99)
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

            //await DownloadReplies(bvid);

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
            var target = fileMgmt.GetVideoFilename(videoInfo, idx, cid);
            logger.LogInformation($"Output Dir:{target.FullName}");

            if (File.Exists(target.FullName))
            {
                logger.LogInformation($"{target.FullName} is already exists.");
            }
            else
            {
                logger.LogTrace($"Downloading {bvid} to {target.FullName}");

                var page = videoInfo.Pages[idx];
                AvMeta meta = new()
                {
                    title = page.Title,
                    artist = videoInfo.Owner.Name!,
                    album = videoInfo.Title!,
                    copyright = videoInfo.Bvid!
                };

                var videoStreamInfo = await blient.GetVideoStream(bvid, cid != -99 ? cid : page.Cid) ?? throw new NotSupportedException(); // videoStreamInfoResp.Data;
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
            logger.LogCritical(ex.ToFullString());
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
            logger.LogDebug(ex.ToFullString());
            return null;
        }
    }

}
