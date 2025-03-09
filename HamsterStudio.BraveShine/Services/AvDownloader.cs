using HamsterStudio.Barefeet.Logging;
using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;
using HamsterStudio.BraveShine.Modelss.Bilibili.SubStruct;
using HamsterStudio.Toolkits.SysCall;
using HamsterStudio.Web.Services;
using System.IO;

namespace HamsterStudio.BraveShine.Services;

public class AvDownloader(BiliApiClient bClient)
{
    public const string BVDHome = @"D:\BVDownload";
    public const string BVCoverHome = @"D:\BVDownload\Covers";

    public async Task<string> Download(AvMeta meta,
        string aurl, string vurl,
        string output,
        bool? DeleteAvCache = true)
    {
        try
        {
            string saving_path = @$"{BVDHome}\dash";
            output = Path.Combine(saving_path, output);
            Logger.Shared.Information($"Output Dir:{output}");

            if (File.Exists(output))
            {
                Logger.Shared.Information($"{output} Exists.");
                return "";
            }

            bClient.Browser.Referer = $"https://www.bilibili.com/video/{meta.copyright}/";
            string aname = await bClient.DownloadFile(aurl, Environment.CurrentDirectory);
            string vname = await bClient.DownloadFile(vurl, Environment.CurrentDirectory);
            MergeAv(vname, aname, meta, output);

            if (DeleteAvCache ?? true)
            {
                try { File.Delete(aname ?? ""); } catch (Exception ex) { Logger.Shared.Critical(ex); }
                try { File.Delete(vname ?? ""); } catch (Exception ex) { Logger.Shared.Critical(ex); }
            }
        }
        catch (Exception ex)
        {
            Logger.Shared.Critical(ex);
        }

        return output;
    }

    public async Task<string> SaveCover(VideoInfo videoInfo)
    {
        return await SaveCover(videoInfo.Bvid, videoInfo.Pic);
    }
    
    public async Task<string> SaveCover(WatchLaterDat watchLater)
    {
        return await SaveCover(watchLater.Bvid, watchLater.Pic);
    }

    public async Task<string> SaveCover(string bvid, PagesItem pagesItem)
    {
        return await SaveCover(bvid, pagesItem.FirstFrame);
    }
    
    public async Task<string> SaveCover(string bvid, string url)
    {
        string filename = url.Split("?")[0].Split("@")[0].Split("/").Last();
        filename = $"{bvid}_bili_{filename}";
        string result = await FileSaver.SaveFileFromUrl(url, BVCoverHome, filename);
        Logger.Shared.Information($"Saved {bvid} cover to {result}");
        return result;
    }

    public void MergeAv(string vname, string aname, AvMeta meta, string outp)
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
