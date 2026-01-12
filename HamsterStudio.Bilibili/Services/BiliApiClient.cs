using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Web;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HamsterStudio.Bilibili.Services;

public class BiliApiClient
{
    public const string Referer = "https://www.bilibili.com/";

    public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    public string Cookies { get; private set; }
    public FileMgmt FileMgmt { get; private set; }

    private readonly ILogger _logger;

    public BiliApiClient(FileMgmt fileMgmt, ILogger<BiliApiClient> logger)
    {
        _logger = logger;
        this.FileMgmt = fileMgmt;
        Cookies = LoadCookies();
        //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
    }

    private string LoadCookies()
    {
        try
        {
            string cookiesFilename = Path.Combine(FileMgmt.StorageHome, "cookies.txt");
            return File.ReadAllText(cookiesFilename);
        }
        catch (Exception ex)
        {
            if (ex is DirectoryNotFoundException or FileNotFoundException)
            {
                _logger.LogWarning("Load cookies failed.");
                return string.Empty;
            }
            throw;
        }
    }

    public async Task<T?> GetApiAsync<T>(string api)
    {
        var browser = new FakeBrowser(_logger)
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
                _logger.LogError($"Request {api} failed, {respp.Message}({respp.Code}).");
            }
            return respp.Data;
        }
        catch (JsonException e)
        {
            _logger.LogCritical(e.ToFullString());

            resp.Seek(0, SeekOrigin.Begin);
            using StreamReader streamReader = new(resp);
            _logger.LogInformation("resp: " + streamReader.ReadToEnd());
        }
        return default;
    }

    public async Task<VideoStreamInfo?> GetVideoStream(string bvid, long cid)
    {
        string api = "https://api.bilibili.com/x/player/wbi/playurl?" + $"fnval=144&cid={cid}&qn=127&bvid={bvid}&fourk=1";
        return await GetApiAsync<VideoStreamInfo>(api);
    }

    public async Task<WatchLaterData?> GetWatchLater()
    {
        const string WatchLaterApi = "https://api.bilibili.com/x/v2/history/toview";
        return await GetApiAsync<WatchLaterData>(WatchLaterApi);
    }

}
