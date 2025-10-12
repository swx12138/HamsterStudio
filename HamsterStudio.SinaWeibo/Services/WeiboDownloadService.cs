using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Barefeet.Task;
using HamsterStudio.SinaWeibo.Models;
using HamsterStudio.SinaWeibo.Services.Restful;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;

namespace HamsterStudio.SinaWeibo.Services;

public class WeiboDownloadService(
    IWeiboApi api,
    CommonDownloader commonDownloader,
    FileMgmt fileMgmt,
    FilenameFormatter formatter)
{
    public event Action<ShowDataModel>? OnShowInfoUpdated;

    private Logger logger = Logger.Shared;

    public async Task<ServerRespModel> Download(string show_id, string referer)
    {
        var model = await api.GetShowInfo(show_id, referer);
        return await Download(model.RetweetedStatus ?? model);
    }

    public async Task<ServerRespModel> Download(ShowDataModel show)
    {
        // TBD：抽象一个通用的下载器，统一文件名规则和存储逻辑

        ArgumentNullException.ThrowIfNull(show, nameof(show));

        OnShowInfoUpdated?.Invoke(show);
        fileMgmt.UserNameMap.UpdateCache(show.User);

        List<string> imageUrlList = [.. GetImageList(show)];
        foreach (var imgUrl in imageUrlList)
        {
            string imgName = imgUrl.Split('?').First().Split('/').LastOrDefault() ?? $"unknown_{Timestamp.NowMs}.jpg";
            var filename = formatter.Format(show.MblogId, show.User.Idstr, imgName, imageUrlList.IndexOf(imgUrl));
            //await DownloadMedia(imgName, filename);

            _ = await commonDownloader.EasyDownloadFileAsync(new(imgUrl), fileMgmt.GetFullPath(filename, show.User.Idstr));
        }

        return new ServerRespModel()
        {
            Status = 0,
            Message = "Ok",
            Data = new()
            {
                AuthorNickName = show.User.ScreenName,
                Description = show.Text,
                //StaticFiles = [.. files.Select(f => $"xiaohongshu/{f}")], // TBD: 加一个SatticFiles的管理器
                Title = show.TextRaw
            }
        };
    }

    public static IEnumerable<string> GetImageList(ShowDataModel show)
    {
        ArgumentNullException.ThrowIfNull(show, nameof(show));
        foreach (var picInfos in show.PicInfos)
        {
            yield return picInfos.Value.Largest.Url;
        }
    }

}
