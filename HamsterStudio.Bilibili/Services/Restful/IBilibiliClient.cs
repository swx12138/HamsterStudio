using HamsterStudio.Bilibili.Models;
using HamsterStudio.Web.DataModels;
using Refit;

namespace HamsterStudio.Bilibili.Services.Restful;

public interface IBilibiliClient
{
    [Post("/bilib/download-video")]
    Task<ServerRespModel> PostDownloadInfo(DownloadVideoRequest request);

}
