using HamsterStudio.Douyin.DataModels;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Utilities;

namespace HamsterStudio.Douyin.Services;

public class DouyinCrawlerService(DouyinResourcesDownloadService downloadService)
{
    public async Task<ServerRespModel> DownloadResourcesBy(string shareUrl)
    {
        var url = shareUrl.Split().First(x => x.StartsWith("http"));
        var realUrl = await RedirectResolver.GetFinalUrlRecursiveAsync(url);
        var postId = realUrl.Split('?').First().Split('/').Last();
        return await DownloadResourcesById(postId);
    }

    public async Task<ServerRespModel> DownloadResourcesById(string postId)
    {
        var post = await AwemePostResolver.Resolve(postId);
        return await DownloadResourcesBy(post);
    }

    public async Task<ServerRespModel> DownloadResourcesBy(WebAwemePostModel post)
    {
        var request = new RequestDataModel(post);
        return await DownloadResourcesBy(request);
    }

    /// <summary>
    /// Controller直接调用此方法
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ServerRespModel> DownloadResourcesBy(RequestDataModel request)
    {
        return await downloadService.DownloadResourcesBy(request);
    }

}
