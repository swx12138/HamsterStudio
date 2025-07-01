using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Bilibili.Models.Sub;
using HamsterStudio.Bilibili.Services.Restful;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Request;
using HamsterStudio.Web.Tools;
using FileInfo = HamsterStudio.Barefeet.FileSystem.FileInfo;

namespace HamsterStudio.Bilibili.Services;

public class BangumiDownloadService(
    IBilibiliApiService bilibiliApi,
    BiliApiClient blient,
    FileMgmt fileMgmt,
    CommonDownloader downloader,
    HttpClientProvider httpClientProvider)
{
    public string Cookies { get; set; } = string.Empty;

    public event Action<VideoInfo> OnVideoInfoUpdated = delegate { };

    public async Task<ServerRespModel> GetVideoByBvid(string bvid, int idx = -1)
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
            var wish_filename = fileMgmt.GetVideoFilename(videoInfo, idx);
            if (!File.Exists(wish_filename.FullName))
            {
                var page = videoInfo.Pages[idx];

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
                var acceptQuality = videoStreamInfo.AcceptQuality.Max();
                var (qua_num, qua, qua_str) = videoStreamInfo.AcceptQuality
                    .Zip(videoStreamInfo.AcceptFormat.Split(','), videoStreamInfo.AcceptDescription)
                    .First(x => x.First == acceptQuality);
                Logger.Shared.Information($"Selected quality {qua}({qua_str}, {qua_num})");



                AvMeta meta = new()
                {
                    title = page.Title,
                    artist = videoInfo.Owner.Name!,
                    album = videoInfo.Title!,
                    copyright = videoInfo.Bvid!
                };

                var avPath = await DownloadStream(videoStreamInfo, acceptQuality, meta.copyright);
                var result = MergeStreamToMp4(meta, avPath, wish_filename, DeleteAvCache: true);

                await SaveCover(videoInfo);
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

    public async Task<(string a, string v)> DownloadStream(VideoStreamInfo streamInfo, int acceptQuality, string bvid)
    {
        var strategy = new AuthenticRequestStrategy(httpClientProvider.HttpClient, msg =>
        {
            msg.Headers.Referrer = new Uri($"https://www.bilibili.com/video/{bvid}/");
        });

        string vBaseUrl = SelectVideoBaseUrl(acceptQuality, streamInfo);
        string vName = vBaseUrl.Filename();
        string vPath = Path.Combine(fileMgmt.TemporaryHome, vName);
        var vrequ = new DownloadRequest(new Uri(vBaseUrl), strategy);
        if (!await downloader.DownloadFileAsync(vrequ, vPath))
        {
            throw new Exception($"Failed to download video stream from {vBaseUrl}");
        }

        string aBaseUrl = SelectAudioBaseUrl(streamInfo);
        string aName = aBaseUrl.Filename();
        string aPath = Path.Combine(fileMgmt.TemporaryHome, aName);
        var arequ = new DownloadRequest(new Uri(aBaseUrl), strategy);
        if (!await downloader.DownloadFileAsync(arequ, aPath))
        {
            throw new Exception($"Failed to download audio stream from {aBaseUrl}");
        }

        return (aPath, vPath);

        static string SelectVideoBaseUrl(int acceptQuality, VideoStreamInfo vsi)
        {
            var lst = vsi.Dash.Video.Where(x => x.Id == acceptQuality)
                .OrderBy(x => x.Bandwidth);
            if (lst == null || !lst.Any())
            {
                lst = vsi.Dash.Video.OrderBy(x => x.Bandwidth);
            }

            Logger.Shared.Information($"Video dash info : {lst?.Last()!.Width}*{lst?.Last()!.Height} bandw:{lst?.Last()!.Bandwidth}");
            return lst?.Last().BaseUrl ?? string.Empty;
        }

        static string SelectAudioBaseUrl(VideoStreamInfo vsi)
        {
            var dash = vsi.Dash.Flac?.Audio ?? vsi.Dash.Audio.OrderBy(x => x.Bandwidth).Last();
            Logger.Shared.Information($"Audio dash info : {dash.Bandwidth}");
            return dash.BaseUrl ?? string.Empty;
        }
    }

    public static BilibiliVideoDownloadResult MergeStreamToMp4(AvMeta meta, (string aPath, string vPath) avPath, FileInfo target, bool? DeleteAvCache = true)
    {
        Logger.Shared.Information($"Output Dir:{target.FullName}");

        if (File.Exists(target.FullName))
        {
            Logger.Shared.Information($"{target.FullName} Exists.");
            return new()
            {
                VideoDest = target,
                State = FileDownloadState.Existed,
            };
        }

        MergeAv(avPath.vPath, avPath.aPath, meta, target.FullName);
        if (DeleteAvCache ?? true)
        {
            try { File.Delete(avPath.aPath); } catch (Exception ex) { Logger.Shared.Critical(ex); }
            try { File.Delete(avPath.vPath); } catch (Exception ex) { Logger.Shared.Critical(ex); }
        }

        return new()
        {
            VideoDest = target,
            State = FileDownloadState.Succeed,
        };
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
            var result = await downloader.EasyDownloadFileAsync(new(url), dest.FullName);
            if (result)
            {
                Logger.Shared.Information($"Saved {bvid} cover to {dest.FullName}");
            }
            else
            {
                Logger.Shared.Warning($"Failed to save {bvid} cover \n\t from {url} \n\t to {dest.FullName}");
            }
            return dest.FullName;
        }
        catch (Exception ex)
        {
            Logger.Shared.Debug(ex);
            return null;
        }
    }

    public static void MergeAv(string vname, string aname, AvMeta meta, string outp)
    {
        string cmd = $"chcp 65001 & ffmpeg -i \"{vname}\" -i \"{aname}\" -c:v copy -c:a copy " +
            $"-metadata title=\"{meta.title}\" " +
            $"-metadata artist=\"{meta.artist}\" " +
            $"-metadata album=\"{meta.album}\" " +
            $"-metadata copyright=\"{meta.copyright}\" " +
            $"\"{outp}\"";
        ExplorerShell.System(cmd);
        Logger.Shared.Information($"Av merge succeed.");
    }

}
