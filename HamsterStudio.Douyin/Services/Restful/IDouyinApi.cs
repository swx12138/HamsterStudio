using HamsterStudio.Douyin.DataModels;
using Refit;

namespace HamsterStudio.Douyin.Services.Restful;

public interface IDouyinApi
{
    [Get("/aweme/v1/web/aweme/post/")]
    Task<string> GetAwemePost(string postId);
}
