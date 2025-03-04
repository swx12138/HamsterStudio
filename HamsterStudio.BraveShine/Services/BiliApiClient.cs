using HamsterStudio.BraveShine.Models.Bilibili;
using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;
using HamsterStudio.Web.Interfaces;
using HamsterStudio.Web.Request;
using HamsterStudio.Web.Services;
using System.IO;
using System.Net;
using System.Text.Json;

namespace HamsterStudio.BraveShine.Services
{
    public class BiliApiClient
    {
        public const string Referer = "https://www.bilibili.com/";
        public FakeBrowser Browser { get; private set; }
        public string Cookies { get; private set; }

        public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
        };

        private IMyLogger Logger { get; }

        public BiliApiClient(string _cookies,IMyLogger logger)
        {
            Logger = logger;
            Cookies = _cookies;
            Browser = new()
            {
                Cookies = Cookies,
                Referer = Referer
            };
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
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
                    Logger.Error($"Request {api} failed, {respp.Message}({respp.Code}).");
                }
                return respp.Data;
            }
            catch (JsonException e)
            {
                Logger.Critical(e);

                resp.Seek(0, SeekOrigin.Begin);
                using StreamReader streamReader = new(resp);
                Logger.Information("resp: " + streamReader.ReadToEnd());
            }
            return default;
        }

        public async Task<VideoInfo?> GetVideoInfo(string bvid)
        {
            //#if DEBUG
            //            string json_data = File.ReadAllText(@"G:\Code\HamsterStudio\BV1ax4y1x7ua_view.json");
            //            return await Task.FromResult(JsonSerializer.Deserialize<Response<VideoInfo>>(json_data).Data);
            //#else
            string api = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";
            return await GetApiAsync<VideoInfo>(api);
            //#endif
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
                Logger.Error(e.Message + "\n" + e.StackTrace);
            }
            return videoInfo != null;
        }

        public async Task<VideoStreamInfo?> GetVideoStream(string bvid, PagesItem page)
        {
            //#if DEBUG
            //            string json_data = File.ReadAllText(@"G:\Code\HamsterStudio\BV1ax4y1x7ua_playurl.json");
            //            var json_resp = JsonSerializer.Deserialize<Response<VideoStreamInfo>>(json_data);
            //            return await Task.FromResult(json_resp.Data);
            //#else
            string api = "https://api.bilibili.com/x/player/wbi/playurl?" + $"fnval=144&cid={page.Cid}&qn=120&bvid={bvid}&fourk=1";
            return await GetApiAsync<VideoStreamInfo>(api);
            //#endif
        }

        public Task<string> DownloadFile(string url, string path, string? filename = null)
        {
            return FileSaver.SaveFileFromUrl(url, path, filename, Browser);
        }

        public async Task<WatchLaterData?> GetWatchLater()
        {
            const string WatchLaterApi = "https://api.bilibili.com/x/v2/history/toview";
            return await GetApiAsync<WatchLaterData>(WatchLaterApi);
        }

    }

}
