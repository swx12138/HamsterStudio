using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.Toolkits.SysCall;
using System.Diagnostics;
using System.IO;

namespace HamsterStudio.BraveShine.Services;

public class AvDownloader(BiliApiClient bClient)
{
    public const string BVDHome = @"D:\BVDownload";

    public async Task<string> Download(AvMeta meta,
        string aurl, string vurl,
        string output,
        bool? DeleteAvCache = true,
        EventHandler<string>? finished = null)
    {
        try
        {
            string saving_path = @$"{BVDHome}\\dash";
            output = Path.Combine(saving_path, output);

            if (File.Exists(output))
            {
                finished?.Invoke(this, $"{output} Exists.");
                return "";
            }

            bClient.Browser.Referer = $"https://www.bilibili.com/video/{meta.copyright}/";
            string aname = await bClient.DownloadFile(aurl, Environment.CurrentDirectory);
            string vname = await bClient.DownloadFile(vurl, Environment.CurrentDirectory);
            MergeAv(vname, aname, meta, output);

            if (DeleteAvCache ?? true)
            {
                try { File.Delete(aname ?? ""); } catch (Exception ex) { Trace.TraceError(ex.Message); }
                try { File.Delete(vname ?? ""); } catch (Exception ex) { Trace.TraceError(ex.Message); }
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError($"[GG] {ex.Message}");
        }

        return output;
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
    }
}
