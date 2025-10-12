using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Douyin.DataModels;
using HamsterStudio.Douyin.Services.Restful;
using Refit;

namespace HamsterStudio.Douyin.Services;

internal static class AwemePostResolver
{
    private static Lazy<IDouyinApi> _douyinApi = new Lazy<IDouyinApi>(() => RestService.For<IDouyinApi>(
        new HttpClient(
            new LoggingHandler(
                new HttpClientHandler()))
        { BaseAddress = new Uri("https://www.douyin.com/") }));

    public static async Task<WebAwemePostModel> Resolve(string postId)
    {
        var str = await _douyinApi.Value.GetAwemePost(postId);
        return new WebAwemePostModel();
    }

}
