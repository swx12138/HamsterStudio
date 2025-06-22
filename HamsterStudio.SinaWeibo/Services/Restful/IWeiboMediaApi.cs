using HamsterStudio.Barefeet.Constants;
using Refit;

namespace HamsterStudio.SinaWeibo.Services.Restful;

[Headers($"User-Agent:{BrowserConsts.EdgeUserAgent}", "Referer:https://weibo.com/")]
public interface IWeiboMediaApi
{
    [Get("/large/{filename}")]
    Task<Stream> GetFile(string filename);

}
