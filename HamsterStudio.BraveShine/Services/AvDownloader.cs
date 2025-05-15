using HamsterStudio.Barefeet.Logging;
using HamsterStudio.BraveShine.Constants;
using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;
using HamsterStudio.BraveShine.Modelss.Bilibili.SubStruct;
using HamsterStudio.Toolkits.SysCall;
using HamsterStudio.Web;
using HamsterStudio.Web.Utilities;
using System.IO;

namespace HamsterStudio.BraveShine.Services;

public class AvDownloader
{
    public async Task<string> Download(AvMeta meta,
        string aurl, string vurl,
        string output,
        bool? DeleteAvCache = true)
    {
        try
        {
            string saving_path = @$"{SystemConsts.BVDHome}\dash";
            output = Path.Combine(saving_path, output);
            Logger.Shared.Information($"Output Dir:{output}");

            if (File.Exists(output))
            {
                Logger.Shared.Information($"{output} Exists.");
                return "";
            }

            var browser = new FakeBrowser();
            browser.Referer = $"https://www.bilibili.com/video/{meta.copyright}/";
            string aname = await FileSaver.SaveFileFromUrl(aurl, Environment.CurrentDirectory, fakeBrowser: browser);
            string vname = await FileSaver.SaveFileFromUrl(vurl, Environment.CurrentDirectory, fakeBrowser: browser);
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

    public static async Task<string> SaveCover(VideoInfo videoInfo)
    {
        return await SaveCover(videoInfo.Bvid, videoInfo.Pic);
    }

    public static async Task<string> SaveCover(WatchLaterDat watchLater)
    {
        return await SaveCover(watchLater.Bvid, watchLater.Pic);
    }

    public static async Task<string> SaveCover(string bvid, PagesItem pagesItem)
    {
        return await SaveCover(bvid, pagesItem.FirstFrame);
    }

    public static async Task<string> SaveCover(string bvid, string url)
    {
        try
        {
            string filename = url.Split("?")[0].Split("@")[0].Split("/").Last();
            filename = $"{bvid}_bili_{filename}";
            string result = await FileSaver.SaveFileFromUrl(url, SystemConsts.BVCoverHome, filename);
            Logger.Shared.Information($"Saved {bvid} cover to {result}");
            return result;
        }
        catch (Exception ex)
        {
            Logger.Shared.Debug(ex);
            return string.Empty;
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
