using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Request;
using System.Threading.Tasks;
using HamstertFileInfo = HamsterStudio.Barefeet.FileSystem.HamstertFileInfo;

namespace HamsterStudio.Bilibili.Services.StreamDownloaders;

internal class DashDownloader(CommonDownloader downloader, FileMgmt fileMgmt, AuthenticRequestStrategy strategy, StreamDownloaderChaeine? inner) : StreamDownloaderChaeine(inner)
{
    public override async Task<DownloadStatus> Download(VideoStreamInfo videoStreamInfo, AvMeta meta, HamstertFileInfo target)
    {
        if (videoStreamInfo.Dash.Video != null && videoStreamInfo.Dash.Audio != null)
        {
            var acceptQuality = videoStreamInfo.AcceptQuality.Max();
            var (qua_num, qua, qua_str) = videoStreamInfo.AcceptQuality
                .Zip(videoStreamInfo.AcceptFormat.Split(','), videoStreamInfo.AcceptDescription)
                .First(x => x.First == acceptQuality);
            Logger.Shared.Information($"Selected quality {qua}({qua_str}, {qua_num})");

            var avPath = await DownloadStream(videoStreamInfo, acceptQuality, meta.copyright);
            var rslt = await MergeStreamToMp4(meta, avPath, target, DeleteAvCache: true);
            return rslt.State;
        }

        return await base.Download(videoStreamInfo, meta, target);
    }

    public async Task<(string a, string v)> DownloadStream(VideoStreamInfo streamInfo, int acceptQuality, string bvid)
    {
        string vBaseUrl = SelectVideoBaseUrl(acceptQuality, streamInfo);
        string vName = vBaseUrl.Filename();
        string vPath = Path.Combine(fileMgmt.TemporaryHome, vName);
        //var vrequ = new DownloadRequest(, , );

        var status = await downloader.DownloadFileAsync(new Uri(vBaseUrl), vPath, strategy, ContentCopyStrategy, DownloadStrategy);
        if (status == DownloadStatus.Failed)
        {
            throw new Exception($"Failed to download video stream from {vBaseUrl}");
        }

        string aBaseUrl = SelectAudioBaseUrl(streamInfo);
        string aName = aBaseUrl.Filename();
        string aPath = Path.Combine(fileMgmt.TemporaryHome, aName);
        status = await downloader.DownloadFileAsync(new Uri(aBaseUrl), aPath, strategy, ContentCopyStrategy, DownloadStrategy);
        if (status == DownloadStatus.Failed)
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

    public static async Task<BilibiliVideoDownloadResult> MergeStreamToMp4(AvMeta meta, (string aPath, string vPath) avPath, HamstertFileInfo target, bool? DeleteAvCache = true)
    {
        if (File.Exists(target.FullName))
        {
            Logger.Shared.Information($"{target.FullName} Exists.");
            return new()
            {
                VideoDest = target,
                State = DownloadStatus.Exists,
            };
        }

        await MergeAv(avPath.vPath, avPath.aPath, meta, target.FullName);
        if (DeleteAvCache ?? true)
        {
            try { File.Delete(avPath.aPath); } catch (Exception ex) { Logger.Shared.Critical(ex); }
            try { File.Delete(avPath.vPath); } catch (Exception ex) { Logger.Shared.Critical(ex); }
        }

        return new()
        {
            VideoDest = target,
            State = DownloadStatus.Success,
        };
    }

    public static async Task MergeAv(string vname, string aname, AvMeta meta, string outp)
    {
        string cmd = $"chcp 65001 & ffmpeg -i \"{vname}\" -i \"{aname}\" -c:v copy -c:a copy " +
            $"-metadata title=\"{meta.title}\" " +
            $"-metadata artist=\"{meta.artist}\" " +
            $"-metadata album=\"{meta.album}\" " +
            $"-metadata copyright=\"{meta.copyright}\" " +
            $"\"{outp}\"";
        ShellApi.System(cmd);
        Logger.Shared.Information($"Av merge succeed.");
    }

}
