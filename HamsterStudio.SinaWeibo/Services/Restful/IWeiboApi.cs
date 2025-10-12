using HamsterStudio.Barefeet.Constants;
using HamsterStudio.SinaWeibo.Models;
using Refit;

namespace HamsterStudio.SinaWeibo.Services.Restful;

[Headers($"User-Agent:{BrowserConsts.EdgeUserAgent}", $"Accept:{BrowserConsts.DefaultAccept}")]
public interface IWeiboApi
{
    [Get("/ajax/statuses/show")]
    [Headers("X-Requested-With: XMLHttpRequest")]

    Task<ShowDataModel> GetShowInfo(string id, [Header("Referer")]string referer, 
        string locale = "zh-CN", bool isGetLongText = true);

}
