using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Web.Services;
using HamsterStudio.Web.Strategies.Request;
using Microsoft.Extensions.Logging;

namespace HamsterStudio.Bilibili.Services.StreamDownloaders;

internal class DashDownloader(CommonDownloader downloader, FileMgmt fileMgmt, AuthenticRequestStrategy strategy, StreamDownloaderChaeine? inner, ILogger? logger) : StreamDownloaderChaeine(inner, logger)
{
    public override async Task<DownloadStatus> Download(VideoStreamInfo videoStreamInfo, AvMeta meta, HamstertFileInfo target)
    {
        if (videoStreamInfo.Dash.Video != null && videoStreamInfo.Dash.Audio != null)
        {
            var acceptQuality = videoStreamInfo.AcceptQuality.Max();
            var (qua_num, qua, qua_str) = videoStreamInfo.AcceptQuality
                .Zip(videoStreamInfo.AcceptFormat.Split(','), videoStreamInfo.AcceptDescription)
                .First(x => x.First == acceptQuality);
            Logger?.LogInformation($"Selected quality {qua}({qua_str}, {qua_num})");

            var avPath = await DownloadStream(videoStreamInfo, acceptQuality, meta.copyright);
            return await MergeStreamToMp4(meta, avPath, target, DeleteAvCache: true);
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

        string SelectVideoBaseUrl(int acceptQuality, VideoStreamInfo vsi)
        {
            var lst = vsi.Dash.Video.Where(x => x.Id == acceptQuality)
                .OrderBy(x => x.Bandwidth);
            if (lst == null || !lst.Any())
            {
                lst = vsi.Dash.Video.OrderBy(x => x.Bandwidth);
            }

            Logger?.LogInformation($"Video dash info : {lst?.Last()!.Width}*{lst?.Last()!.Height} bandw:{lst?.Last()!.Bandwidth}");
            return lst?.Last().BaseUrl ?? string.Empty;
        }

        string SelectAudioBaseUrl(VideoStreamInfo vsi)
        {
            var dash = vsi.Dash.Flac?.Audio ?? vsi.Dash.Audio.OrderBy(x => x.Bandwidth).Last();
            Logger?.LogInformation($"Audio dash info : {dash.Bandwidth}");
            return dash.BaseUrl ?? string.Empty;
        }
    }

    public async Task<DownloadStatus> MergeStreamToMp4(AvMeta meta, (string aPath, string vPath) avPath, HamstertFileInfo target, bool? DeleteAvCache = true)
    {
        if (File.Exists(target.FullName))
        {
            Logger?.LogInformation($"{target.FullName} Exists.");
            return DownloadStatus.Exists;
        }

        await MergeAv(avPath.vPath, avPath.aPath, meta, target.FullName);
        if (DeleteAvCache ?? true)
        {
            try { File.Delete(avPath.aPath); } catch (Exception ex) { Logger?.LogCritical(ex.ToFullString()); }
            try { File.Delete(avPath.vPath); } catch (Exception ex) { Logger?.LogCritical(ex.ToFullString()); }
        }

        return DownloadStatus.Success;
    }

    public async Task MergeAv(string vname, string aname, AvMeta meta, string outp)
    {
        string cmd = $"chcp 65001 & ffmpeg -i \"{vname}\" -i \"{aname}\" -c:v copy -c:a copy " +
            $"-metadata title=\"{meta.title}\" " +
            $"-metadata artist=\"{meta.artist}\" " +
            $"-metadata album=\"{meta.album}\" " +
            $"-metadata copyright=\"{meta.copyright}\" " +
            $"\"{outp}\"";
        ShellApi.System(cmd);
        Logger?.LogInformation($"Av merge succeed.");
    }

}
