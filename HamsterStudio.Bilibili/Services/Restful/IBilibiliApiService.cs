using HamsterStudio.Bilibili.Constants;
using HamsterStudio.Bilibili.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Bilibili.Services.Restful;

[Headers("User-Agent:Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36 Edg/136.0.0.0",
    "Referer:https://www.bilibili.com/")]
public interface IBilibiliApiService
{
    [Get("/x/web-interface/view")]
    Task<Response<VideoInfo>> GetVideoInfoAsync([AliasAs("bvid")] string bvid);

    [Get("/x/player/wbi/playurl")]
    Task<Response<VideoStreamInfo>> GetVideoStreamInfoAsync([AliasAs("cid")] long cid,
                                                            [AliasAs("bvid")] string bvid,
                                                            [Header("Cookies")] string cookies,
                                                            [AliasAs("fnval")] int fnval = 144,
                                                            [AliasAs("qn")] int qn = 120,
                                                            [AliasAs("fourk")] int fourk = 1);
    [Get("/x/v2/history/toview")]
    Task<Response<WatchLaterData>> GetWatchLaterAsync([Header("Cookies")] string cookies,
                                                      [Header("Accept")] string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");

    [Get(SystemConsts.ConclusionPath)]
    Task<Response<ConclusionModel>> GetConclusionView(string bvid,
                                                      string cid,
                                                      string up_mid);

}

