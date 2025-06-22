using HamsterStudio.Barefeet.Constants;
using HamsterStudio.SinaWeibo.Models;
using Refit;

namespace HamsterStudio.SinaWeibo.Services.Restful;

[Headers($"User-Agent:{BrowserConsts.EdgeUserAgent}",$"Accept:{BrowserConsts.DefaultAccept}")]
public interface IWeiboApi
{
    [Get("/ajax/statuses/show")]
    Task<ShowDataModel> GetShowInfo(string id, 
                             string locale = "zh-CN",
                             bool isGetLongText = true);

}
