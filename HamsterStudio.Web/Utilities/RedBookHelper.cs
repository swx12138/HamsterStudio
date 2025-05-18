using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Models;
using HamsterStudio.Web.DataModels.ReadBook;
using HamsterStudio.Web.Interfaces;
using HtmlAgilityPack;
using System.Text.Json;

namespace HamsterStudio.Web.Utilities;

public static class RedBookHelper
{
    public delegate bool CheckOldDirFunc(string filename);

    private static string SelectTitle(NoteDetailModel noteDetail)
    {
        if (noteDetail.Title != string.Empty)
        {
            return noteDetail.Title;
        }
        return noteDetail.Time.ToString();
    }

    public static async Task<ServerResp> Download(NoteDataModel noteData, string storageDir, CheckOldDirFunc? checkOldDirFunc = null)
    {
        var currentNote = noteData.NoteDetailMap[noteData.CurrentNoteId];
        Logger.Shared.Information($"开始处理<{GetTypeName(currentNote.NoteDetail)}>作品：{noteData.CurrentNoteId}");
        Logger.Shared.Information($"标题：{currentNote.NoteDetail.Title}【{currentNote.NoteDetail.ImageList.Count}】");

        CommonDownloader downloader = new();
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
            string filename = GenerateFilename(
                SelectTitle(currentNote.NoteDetail),
                index,
                currentNote.NoteDetail.UserInfo,
                token);
            if (checkOldDirFunc != null && checkOldDirFunc(filename))
            {
                Logger.Shared.Information($"[{index}/{currentNote.NoteDetail.ImageList.Count}]{filename} 早已存在.");
                continue;
            }

            string url = GeneratePngLink(imgInfo.DefaultUrl);
            try
            {
                DownloadResukt result = await DownloadFile(url, storageDir, filename, downloader, (imgInfo.Width, imgInfo.Height));
                if (result.Success)
                {
                    contained_files.Add(filename);
                    await Task.Delay(50 * Random.Shared.Next(5));
                }
                else
                {
                    Logger.Shared.Error($"[{index}/{currentNote.NoteDetail.ImageList.Count}]下载失败。");
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
            string video_url = GenerateVideoLink(currentNote.NoteDetail.VideoInfo.Consumer.OriginVideoKey);
            string filename = GenerateVideoFilename(SelectTitle(currentNote.NoteDetail), currentNote.NoteDetail.UserInfo, currentNote.NoteDetail.VideoInfo.Consumer.OriginVideoKey.Split('/').Last());
            try
            {
                DownloadResukt result = await DownloadFile(video_url, storageDir, filename, downloader);
                if (result.Success)
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

    public static async Task<DownloadResukt> DownloadFile(string url, string storageDir, string filename, IDownloader downloader, (int w, int h)? size = null)
    {
        // Faile @ Ciallo～(∠・ω< )⌒★_0_xhs_咖鱼鱼_1040g3k031h27q5dk382048nlebhc4r1dk1iipeg.png
        var reqMsg = FakeBrowser.CommonClient.CreateRequest(HttpMethod.Get, url);
        reqMsg.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        reqMsg.Headers.Add("Range", "bytes=0-");

        DownloadResukt result = await downloader.DownloadAsync(url, storageDir, filename, httpRequest: reqMsg);
        if (size != null)
        {
            Logger.Shared.Information($"{result}【{size?.w}, {size?.h}】下载成功。");
        }
        else
        {
            Logger.Shared.Information($"{result} 下载成功。");
        }
        return result;
    }

    public static string GetTypeName(NoteDetailModel noteDetail)
    {
        return noteDetail.Type switch
        {
            "normal" => "图文",
            "video" => "视频",
            _ => "Unkonwn"
        };
    }

    public static string GenerateFilename(string tiltle, int? index, UserInfoModel userInfo, string token)
    {
        return $"{tiltle}_{index}_xhs_{userInfo.Nickname}_{token}.png";
    }

    public static string GenerateVideoFilename(string tiltle, UserInfoModel userInfo, string token)
    {
        // TBD:自动判断类型
        return $"{tiltle}_xhs_{userInfo.Nickname}_{token}.mp4";
    }

    public static void DumpJson(string path, NoteDataModel noteData)
    {
        try
        {
            string text = JsonSerializer.Serialize(noteData);
            File.WriteAllText(path, text);
        }
        catch (Exception ex)
        {
            Logger.Shared.Warning($"Dump json failed.{ex.Message}\n{ex.StackTrace}");
        }
    }

    public static string GeneratePngLink(string baseUrl)
    {
        string token = baseUrl.Split("!").First();
        token = token.Substring(token.IndexOf5th('/') + 1);
        return $"https://ci.xiaohongshu.com/{token}?imageView2/format/png";
    }

    public static string GenerateVideoLink(string token)
    {
        //return $"https://sns-img-bd.xhscdn.com/{token}";
        return $"https://sns-video-bd.xhscdn.com/{token}";
    }

    public static NoteDataModel? GetNoteData(in string url)
    {
        FakeBrowser.CommonClient.Referer = "https://www.xiaohongshu.com/explore";
        var redirectedUrl = FakeBrowser.CommonClient.GetRedirectedUrlAsync(url).Result;

        var htmlDoc = new HtmlWeb().Load(redirectedUrl);
        var ndata = GetNote(htmlDoc);
        if (ndata.CurrentNoteId == null)
        {
            htmlDoc.Save("lastest.html");
            return null;
        }
        return ndata;
    }

    private static NoteDataModel? GetNote(HtmlDocument html)
    {
        var initState = GetInitialState(html);
        JsonElement root = initState.RootElement;

        // 访问元素
        if (!root.TryGetProperty("note", out JsonElement noteElement))
        {
            return null;
        }

        return noteElement.Deserialize<NoteDataModel>();
    }

    private static JsonDocument GetInitialState(HtmlDocument html)
    {
        // 查找所有script标签
        var scriptNodes = html.DocumentNode.SelectNodes("//script");

        if (scriptNodes == null)
        {
            throw new Exception("未找到任何script标签");
        }

        foreach (var scriptNode in scriptNodes)
        {
            var scriptContent = scriptNode.InnerText;

            // 查找包含__INITIAL_STATE__的script
            if (scriptContent.Contains("window.__INITIAL_STATE__"))
            {
                // 提取JSON部分
                var jsonStart = scriptContent.IndexOf("{");
                var jsonEnd = scriptContent.LastIndexOf("}") + 1;

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonString = scriptContent.Substring(jsonStart, jsonEnd - jsonStart);
#if DEBUG
                    File.WriteAllText("lastest_init_state.json", jsonString);
#endif

                    jsonString = jsonString.Replace("undefined", "null");
                    try
                    {
                        JsonDocument document = JsonDocument.Parse(jsonString);
                        return document;
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText("lastest_init_state.json", jsonString);
                        throw new Exception("解析JSON失败", ex);
                    }
                }
            }
        }

        throw new Exception("未找到window.__INITIAL_STATE__");
    }
}
