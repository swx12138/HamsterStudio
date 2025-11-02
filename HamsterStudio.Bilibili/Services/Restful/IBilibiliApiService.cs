using HamsterStudio.Bilibili.Constants;
using HamsterStudio.Bilibili.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
                                                            [Header("Cookie")] string cookies,
                                                            [AliasAs("fnval")] int fnval = 144,
                                                            [AliasAs("qn")] int qn = 120,
                                                            [AliasAs("fourk")] int fourk = 1);
    [Get("/x/v2/history/toview")]
    Task<Response<WatchLaterData>> GetWatchLaterAsync([Header("Cookies")] string cookies,
                                                      [Header("Accept")] string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");

    [Get(SystemConsts.ConclusionPath)]
    Task<Response<ConclusionModel>> GetConclusionView(string bvid,
                                                      string cid,
                                                      string up_mid,
                                                      [Header("Cookie")] string cookie);

    [Post("/bapis/bilibili.api.ticket.v1.Ticket/GenWebTicket")]
    Task<Response<BiliTicketModel>> GenWebTicket([AliasAs("hexsign")] string hexSign,
                                                 [AliasAs("context[ts]")] string context_ts,
                                                 [AliasAs("csrf")] string csrf = "",
                                                 [AliasAs("key_id")] string keyId = "ec02");

    [Get("/x/polymer/web-dynamic/v1/detail")]
    Task<Response<DynamicDetailDataModel>> GetDynamicDetail([AliasAs("id")] string dynamic_id);

    [Get("/x/v2/reply")]
    Task<Response<ReplayV2DataModel>> GetReplayV2([AliasAs("oid")] string bvid, int pn, [Header("Cookie")] string cookies, int type = 1);
}

// https://api.vc.bilibili.com
public interface IVcBiliApiService
{
    [Get("/dynamic_repost/v1/dynamic_repost/repost_detail")]
    Task<Response<DynamicDataModel>> GetDynamicRepostDetail(string dynamic_id);

}
