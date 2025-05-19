using HamsterStudio.Barefeet.Interfaces;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels.ReadBook;
using HamsterStudio.Web.Tools;
using System.Net;

namespace HamsterStudio.Web.Utilities;

public class RedBookDownloader
{
    private readonly DownloadStrategyFactory _strategyFactory = new();

    public async Task<ServerResp> Download(NoteDataModel noteData, string storageDir)
    {
        var currentNote = noteData.NoteDetailMap[noteData.CurrentNoteId];
        Logger.Shared.Information($"开始处理<{RedBookHelper.GetTypeName(currentNote.NoteDetail)}>作品：{noteData.CurrentNoteId}");
        Logger.Shared.Information($"标题：{currentNote.NoteDetail.Title}【{currentNote.NoteDetail.ImageList.Count}】");
        List<string> contained_files = [];
        foreach (var imgInfo in currentNote.NoteDetail.ImageList)
        {
            if (imgInfo.LivePhoto)
            {
                Logger.Shared.Warning("暂时不支持LivePhoto！");
                continue;
            }

            string token = imgInfo.DefaultUrl.Split("!").First().Split("/").Where(x => x != null && x.Length > 0).Last();
            int index = currentNote.NoteDetail.ImageList.IndexOf(imgInfo) + 1;
            string filename = RedBookHelper.GenerateFilename(
               RedBookHelper.SelectTitle(currentNote.NoteDetail),
                index,
                currentNote.NoteDetail.UserInfo,
                token);

            string url = RedBookHelper.GeneratePngLink(imgInfo.DefaultUrl);
            try
            {
                switch (await DownloadMedia(url, filename, storageDir))
                {
                    case FileDownlaodState.Succeed:
                        contained_files.Add(filename);
                        Logger.Shared.Information($"{filename}【{imgInfo.Width}, {imgInfo.Height}】下载成功。");
                        await Task.Delay(50 * Random.Shared.Next(5));
                        break;
                    case FileDownlaodState.Existed:
                        contained_files.Add(filename);
                        break;
                    default:
                        Logger.Shared.Error($"[{index}/{currentNote.NoteDetail.ImageList.Count}]下载失败。");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Shared.Error($"[{index}/{currentNote.NoteDetail.ImageList.Count}]下载异常。\n{ex.Message}");
                Logger.Shared.Debug(ex);
            }
        }

        if ("video" == currentNote.NoteDetail.Type)
        {
            string video_url = RedBookHelper.GenerateVideoLink(currentNote.NoteDetail.VideoInfo.Consumer.OriginVideoKey);
            string filename = RedBookHelper.GenerateVideoFilename(
                RedBookHelper.SelectTitle(currentNote.NoteDetail),
                currentNote.NoteDetail.UserInfo,
                currentNote.NoteDetail.VideoInfo.Consumer.OriginVideoKey.Split('/').Last());
            try
            {
                if (await DownloadMedia(video_url, filename, storageDir, true) == FileDownlaodState.Succeed)
                {
                    contained_files.Add(filename);
                    Logger.Shared.Information($"视频{filename}下载成功。");
                }
            }
            catch (Exception ex)
            {
                Logger.Shared.Error($"视频{filename}下载失败。{ex.Message}");
                Logger.Shared.Debug(ex);
            }
        }

        Logger.Shared.Information("Done.");

        return new ServerResp
        {
            Message = "ok",
            Data = new ServerRespData
            {
                Title = currentNote.NoteDetail.Title,
                Description = currentNote.NoteDetail.Description,
                AuthorNickName = currentNote.NoteDetail.UserInfo.Nickname,
                StaticFiles = [.. contained_files.Select(x => "http://192.168.0.101:8899/static/xiaohongshu/" + x)],
            }
        };
    }

    private async Task<FileDownlaodState> DownloadMedia(string url, string filename, string storageDir, bool isVideo = false)
    {
        var fullPath = Path.Combine(storageDir, filename);
        if(File.Exists(fullPath))
        {
            Logger.Shared.Warning($"{filename}已存在。");
            return FileDownlaodState.Existed;
        }

        var requ = new DownloadRequest(
            new Uri(url),
            isVideo ? GetVideoHeaders() : GetImageHeaders(),
            MaxConnections: isVideo ? 4 : 1);
        var strategy = _strategyFactory.CreateStrategy(requ.MaxConnections);

        var downloader = new Downloader(strategy);
        var result = await downloader.ExecuteDownload(requ);

        if (result.StatusCode == HttpStatusCode.OK)
        {
            await File.WriteAllBytesAsync(fullPath, result.Data);
            return FileDownlaodState.Succeed;
        }

        throw new HttpRequestException($"下载失败: {result.StatusCode}");
    }

    private static Dictionary<string, string> GetImageHeaders() => new()
    {
        ["accept"] = "image/webp,image/apng,image/*,*/*;q=0.8",
        ["referer"] = "https://www.xiaohongshu.com/"
    };

    private Dictionary<string, string> GetVideoHeaders()
    {
        return new()
        {
            ["accept"] = "video/mp4,video/webm,video/*",
            ["referer"] = "https://www.xiaohongshu.com/"
        };
    }
}
