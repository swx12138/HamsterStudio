using HamsterStudio.Bilibili.Models;
using HamsterStudio.Bilibili.Models.Live;
using Refit;

namespace HamsterStudio.Bilibili.Services.Restful;

// https://api.live.bilibili.com
public interface IBilibiliLiveApiSevice
{
    [Get("/room/v1/Room/getRoomInfoOld")]
    Task<Response<RoomInfoOld>> GetRoomInnfoOld(long mid);

    [Get("/room/v1/Room/playUrl")]
    Task<Response<PlayUrlData>> GetPlayUrl([AliasAs("cid")] long roomId, string platform = "web", int quality = 2);

}
