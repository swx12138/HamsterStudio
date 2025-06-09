using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Constants;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Bilibili.Models.Sub;
using HamsterStudio.Web;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace HamsterStudio.Bilibili.Services;

public class BiliApiClient
{
    public const string Referer = "https://www.bilibili.com/";
    public string Cookies { get; private set; }

    public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    private ConcurrentDictionary<string, VideoInfo> VideoInfoCache { get; } = [];

    public BiliApiClient(string? _cookies)
    {
        Cookies = _cookies ?? LoadCookies();
        ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
    }

    private static string LoadCookies()
    {
        try
        {
            string cookiesFilename = Path.Combine(SystemConsts.BVDHome, "cookies.txt");
            return File.ReadAllText(cookiesFilename);
        }
        catch (Exception ex)
        {
            if (ex is DirectoryNotFoundException or FileNotFoundException)
            {
                Logger.Shared.Warning("Load cookies failed.");
                return string.Empty;
            }
            throw;
        }
    }

    public async Task<T?> GetApiAsync<T>(string api)
    {
        var browser = new FakeBrowser()
        {
            Cookies = Cookies,
            Referer = Referer
        };
        var resp = await browser.GetStream(api);
        try
        {
            var respp = await JsonSerializer.DeserializeAsync<Response<T>>(resp, JsonSerializerOptions);
            if (respp.Code != 0)
            {
                Logger.Shared.Error($"Request {api} failed, {respp.Message}({respp.Code}).");
            }
            return respp.Data;
        }
        catch (JsonException e)
        {
            Logger.Shared.Critical(e);

            resp.Seek(0, SeekOrigin.Begin);
            using StreamReader streamReader = new(resp);
            Logger.Shared.Information("resp: " + streamReader.ReadToEnd());
        }
        return default;
    }

    public async Task<VideoInfo?> GetVideoInfo(string bvid)
    {
        if (VideoInfoCache.TryGetValue(bvid, out VideoInfo? value))
        {
            return value;
        }
        else
        {
            string api = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";
            value = await GetApiAsync<VideoInfo>(api);
            if (value != null)
            {
                VideoInfoCache[bvid] = value;
            }
            return value;
        }
    }

    public bool TryGetVideoInfo(string bvid, out VideoInfo? videoInfo)
    {
        videoInfo = null;
        try
        {
            videoInfo = GetVideoInfo(bvid).Result;
        }
        catch (Exception e)
        {
            Logger.Shared.Error(e.Message + "\n" + e.StackTrace);
        }
        return videoInfo != null;
    }

    public async Task<VideoStreamInfo?> GetVideoStream(string bvid, long cid)
    {
        string api = "https://api.bilibili.com/x/player/wbi/playurl?" + $"fnval=144&cid={cid}&qn=120&bvid={bvid}&fourk=1";
        return await GetApiAsync<VideoStreamInfo>(api);
    }

    public async Task<WatchLaterData?> GetWatchLater()
    {
        const string WatchLaterApi = "https://api.bilibili.com/x/v2/history/toview";
        return await GetApiAsync<WatchLaterData>(WatchLaterApi);
    }

}
