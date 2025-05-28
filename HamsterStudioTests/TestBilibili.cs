using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Bilibili.Constants;
using HamsterStudio.Bilibili.Services.Restful;
using Refit;

namespace HamsterStudioTests
{
    [TestClass]
    public sealed class TestBilibili
    {
        string cookies = File.ReadAllText(Path.Combine(SystemConsts.BVDHome, "cookies.txt"));
        IBilibiliApiService bapi = RestService.For<IBilibiliApiService>(new HttpClient(new LoggingHandler(new HttpClientHandler(), msg => Console.WriteLine($"[{msg.Method}] {msg.RequestUri} [[{msg.Headers}]]")))
        {
            BaseAddress = new Uri("https://api.bilibili.com")
        });

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
    }
}
