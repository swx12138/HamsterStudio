using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels.ReadBook;
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
                imgInfo,
                SelectTitle(currentNote.NoteDetail),
                index,
                currentNote.NoteDetail.UserInfo,
                token);
            if (checkOldDirFunc != null && checkOldDirFunc(filename))
            {
                Logger.Shared.Information($"[{index}/{currentNote.NoteDetail.ImageList.Count}]{filename} 早已存在.");
                continue;
            }

            try
            {
                string url = GeneratePngLink(imgInfo.DefaultUrl);
                await DownloadFile(url, storageDir, filename, (imgInfo.Width, imgInfo.Height));
                await Task.Delay(50 * Random.Shared.Next(5));
            }
            catch (Exception exx)
            {
                Logger.Shared.Warning($"[{index}/{currentNote.NoteDetail.ImageList.Count}]下载失败：{exx.Message}");
            }
        }

        Logger.Shared.Information("Done.");

        // TBD:static file server
        return new ServerResp
        {
            Message = "ok",
            Data = new ServerRespData
            {
                Title = currentNote.NoteDetail.Title,
                Description = currentNote.NoteDetail.Description,
                AuthorNickName = currentNote.NoteDetail.UserInfo.Nickname,
                StaticFiles = [],
            }
        };
    }

    public static async Task DownloadFile(string url, string storageDir, string filename, (int w, int h)? size = null)
    {
        // Faile @ Ciallo～(∠・ω< )⌒★_0_xhs_咖鱼鱼_1040g3k031h27q5dk382048nlebhc4r1dk1iipeg.png
        var reqMsg = FakeBrowser.CommonClient.CreateRequest(HttpMethod.Get, url);
        reqMsg.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        reqMsg.Headers.Add("Range", "bytes=0-");

        string result = FileSaver.SaveFileFromUrl(url, storageDir, filename, httpRequest: reqMsg).Result;
        if (size != null)
        {
            Logger.Shared.Information($"{result}【{size?.w}, {size?.h}】下载成功。");
        }
        else
        {
            Logger.Shared.Information($"{result} 下载成功。");
        }
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

    public static string GenerateFilename(ImageListItemModel imageListItem, string tiltle, int index, UserInfoModel userInfo, string token)
    {
        // sanitize_windows_path(f'{name["作品标题"]}_{idxxx}_xhs_{name["作者昵称"]}_{____token}')
        return $"{tiltle}_{index}_xhs_{userInfo.Nickname}_{token}.png";
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
        return $"https://sns-img-bd.xhscdn.com/{token}";
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
