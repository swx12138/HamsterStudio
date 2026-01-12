using HamsterStudio.Bilibili;
using HamsterStudio.Bilibili.Models;
using HamsterStudio.Bilibili.Services;
using HamsterStudio.Bilibili.Services.Restful;
using System.Web;

namespace HamsterStudioTests.Bilibili
{
    [TestClass]
    public sealed class TestBilibili
    {
        string cookies = File.ReadAllText(@"E:\HamsterStudioHome\Bilibili\cookies.txt");
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

        [TestMethod]
        public async Task TestGetReplayV2()
        {
            var vinfo = await bapi.GetVideoInfoAsync("BV1MZxnzsEuo");

            Assert.IsNotNull(vinfo);
            Console.WriteLine($"[{vinfo.Code}] {vinfo.Message}");

            Assert.IsNotNull(vinfo.Data);
            Console.WriteLine($"avid:{vinfo.Data.Aid} bvid:{vinfo.Data.Bvid}");
            for (int page = 1; ; page++)
            {
                Console.WriteLine($"Load page {page} ...");
                var replayResp = await bapi.GetReplayV2(vinfo.Data!.Bvid, page, cookies);

                Assert.IsNotNull(replayResp);
                Assert.IsNotNull(replayResp.Data);
                ShowCommentsPage(replayResp.Data.Page);

                if(replayResp.Data.Replies == null)
                {
                    break;
                }

                foreach (var replay in replayResp.Data.Replies)
                {
                    ShowReply(replay);
                }
            }

        }

        private void ShowCommentsPage(PageModel page)
        {
            Console.WriteLine($"Page {page.Num} size:{page.Size} count:{page.Count} acount:{page.Acount}");
        }

        private void ShowReply(RepliesItemModel reply)
        {
            if (reply.UpAction.Like || reply.UpAction.Reply)
            {
                Console.WriteLine($"[{reply.UpAction.Like} {reply.UpAction.Reply}] {reply.Member.Uname} {reply.Content.Pictures.Length}");
            }
        }

    }
}
