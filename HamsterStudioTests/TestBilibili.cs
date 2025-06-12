using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili;
using HamsterStudio.Bilibili.Constants;
using HamsterStudio.Bilibili.Services;
using HamsterStudio.Bilibili.Services.Restful;
using Refit;
using System.Web;

namespace HamsterStudioTests
{
    [TestClass]
    public sealed class TestBilibili
    {
        const string cookies = "enable_web_push=DISABLE; buvid4=D03E4D6A-5698-2F50-705C-9549A16A515475537-024051806-941xmmeJT0vtfn1cUje7pg%3D%3D; rpdid=|(u))|J)|RRY0J'u~uY|uRu|R; header_theme_version=CLOSE; buvid_fp_plain=undefined; LIVE_BUVID=AUTO2117162089414294; is-2022-channel=1; CURRENT_BLACKGAP=0; opus-goback=1; go-old-space=1; enable_feed_channel=ENABLE; historyviewmode=grid; CURRENT_QUALITY=120; PVID=1; SESSDATA=76cb6115%2C1760530701%2Cdb175%2A42CjCNCj5nsl4CkcEjQeLYN3hevGbGMDFGpQ0cjgfUaTwuOc_XGX5S3AIo2FRfxHfgplESVmtxSm5kWEgzcEMyTDhLRWV1Ykw0YUNhLTZGY0pOdFpxczNtM3NBUGlhNFlLdlhRZjZVdkZSeF9OZWxQRTFQZXhzcTg5Z05IV0s1NE5NMEpndlVCaGlBIIEC; bili_jct=fdd128a38522c7fbc817bd777b2763bf; DedeUserID=225655964; DedeUserID__ckMd5=51a2e7cba9fbbf9e; fingerprint=31c734103c43608ee78a878e843f04f9; buvid3=3D7677FB-DAA0-B9A7-AE9C-4E0FC99CA9F082352infoc; b_nut=1747550082; _uuid=C98E10343-5E9A-10C62-5FF6-C1010E2CA92FCA94782infoc; hit-dyn-v2=1; buvid_fp=2fe05f295e2da86e5b76512adf8822ae; home_feed_column=5; bili_ticket=eyJhbGciOiJIUzI1NiIsImtpZCI6InMwMyIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3NDk3NDAwODUsImlhdCI6MTc0OTQ4MDgyNSwicGx0IjotMX0.9DwzXpLzksVHmZMFobUDqNgnwvIMTg3CSIRGk1eUwfE; bili_ticket_expires=1749740025; browser_resolution=1871-1062; sid=5bnsgajr; CURRENT_FNVAL=4048; bp_t_offset_225655964=1077187165160472576; b_lsid=52F109559_1975F062195";
        readonly IBilibiliApiService bapi = WebApiExtensions.CreateServ();

        [TestMethod]
        public async Task TestGetVideoInfoApi()
        {
            var videoInfoResp = await bapi.GetVideoInfoAsync("BV1SsjdzaEv5");
            Assert.IsTrue(videoInfoResp.Code == 0, $"Error: {videoInfoResp.Message}");

            var videoInfo = videoInfoResp.Data;
            Assert.IsNotNull(videoInfo, "Video info should not be null.");

            Console.WriteLine($"Title: {videoInfo.Title}");
            Console.WriteLine($"Description: {videoInfo.Desc}");
            Console.WriteLine($"Description V2: {videoInfo.DescV2}");
            Console.WriteLine($"View Count: {videoInfo.Stat.View}");

            var streamInfoResp = await bapi.GetVideoStreamInfoAsync(videoInfo.Pages.First().Cid, videoInfo.Bvid, cookies);
            Assert.IsTrue(streamInfoResp.Code == 0, $"Error: {streamInfoResp.Message}");

            var streamInfo = streamInfoResp.Data;
            Assert.IsNotNull(streamInfo, "Stream info should not be null.");

            Console.WriteLine($"Stream Count: {streamInfo.Dash.Video.Count}");
            foreach (var desc in streamInfo.AcceptDescription)
            {
                Console.WriteLine($"Stream Description: {desc}");
            }
            Console.WriteLine($"First Stream URL: {streamInfo.Dash.Video.First().BaseUrl}");
        }

        [TestMethod]
        public async Task TestGetWatchLaterApi()
        {
            var watchLaterResp = await bapi.GetWatchLaterAsync(cookies);
            Assert.AreEqual(0, watchLaterResp.Code, $"Error: {watchLaterResp.Message}");

            var watchLaterData = watchLaterResp.Data;
            Assert.IsNotNull(watchLaterData, "Watch Later data should not be null.");

            Console.WriteLine($"Watch Later Count: {watchLaterData.List.Count}");
            foreach (var item in watchLaterData.List)
            {
                Console.WriteLine($"Watch Later Item:【{item.Bvid}】{item.Title}");
            }
        }

        [TestMethod]
        public async Task TestGetConclusionViewApi()
        {
            var conclusionResp = await bapi.GetConclusionView("BV1L94y1H7CV", "1335073288", "297242063", cookies);
            Assert.IsTrue(conclusionResp.Code == 0, $"Error: {conclusionResp.Message}");
            var conclusionData = conclusionResp.Data;
            Assert.IsNotNull(conclusionData, "Conclusion data should not be null.");
        }

        [TestMethod]
        public void TestBsignMixKey()
        {
            string mix_key = BpiSign.MapMixinKey("7cd084941338484aae1ad9425b84077c", "4932caff0ff746eab6f01bf08b70ac45");
            Assert.AreEqual("ea1db124af3c7062474693fa704f4ff8", mix_key);
        }

        [TestMethod]
        public void TestBsignSign()
        {
            const string query_str = "bvid=BV1L94y1H7CV&cid=1335073288&up_mid=297242063";
            var query = HttpUtility.ParseQueryString(query_str);

            string wts = "1701546363";
            string w_rid = BpiSign.GetWrid(query, wts);

            Assert.AreEqual("1073871926b3ccd99bd790f0162af634", w_rid);
        }

        [TestMethod]
        public async Task TestDynamic()
        {
            var resp = await bapi.GetDynamicDetail("967717348014293017");
            Assert.AreEqual(0, resp.Code);

            var data = resp.Data;
            Assert.IsNotNull(data);

        }

    }
}
