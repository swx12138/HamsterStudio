using HamsterStudio.RedBook.DataModels;
using HamsterStudio.RedBook.Interfaces;
using HamsterStudio.Web;
using HtmlAgilityPack;
using System.IO;
using System.Text.Json;

namespace HamsterStudio.RedBook.Services.Parsing;

public class RedBookNoteParser(FakeBrowser? browser = null) : IRedBookParser
{
    private readonly FakeBrowser Browser = browser ?? FakeBrowser.CommonClient;

    public NoteDataModel? GetNoteData(string url)
    {
        Browser.Referer = "https://www.xiaohongshu.com/explore";
        var redirectedUrl = FakeBrowser.CommonClient.GetRedirectedUrlAsync(url).Result;

        var htmlDoc = new HtmlWeb().Load(redirectedUrl);
        var ndata = GetNote(htmlDoc);
        if (ndata == null || ndata.CurrentNoteId == null)
        {
            htmlDoc.Save("lastest.html");
            return null;
        }
        return ndata;
    }

    private NoteDataModel? GetNote(HtmlDocument html)
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

    private JsonDocument GetInitialState(HtmlDocument html)
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
