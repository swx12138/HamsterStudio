using HamsterStudio.Barefeet.Logging;
using HamsterStudio.SinaWeibo.Models;
using HamsterStudio.SinaWeibo.Services.Restful;
using HamsterStudio.Web.DataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.SinaWeibo.Services;

public class DownloadService(IWeiboApi api, IWeiboMediaApi mediaApi)
{
    public event Action<ShowDataModel> OnShowInfoUpdated;

    private FilenameFormatter formatter = new();
    private Logger logger = Logger.Shared;

    public async Task<ServerRespModel> Download(string show_id)
    {
        var model = await api.GetShowInfo(show_id);
        return await Download(model);
    }

    public async Task<ServerRespModel> Download(ShowDataModel show)
    {
        // TBD：抽象一个通用的下载器，统一文件名规则和存储逻辑

        ArgumentNullException.ThrowIfNull(show, nameof(show));

        List<string> imageUrlList = [.. GetImageList(show)];
        foreach(var imgUrl in imageUrlList)
        {
            string imgName = imgUrl.Split('/').LastOrDefault() ?? "unknown.jpg";
            var filename = formatter.Format(show.MblogId, show.User.Idstr, imgName, imageUrlList.IndexOf(imgUrl));
            await DownloadMedia(imgName, filename);
        }

        return new ServerRespModel()
        {

        };
    }

    public async Task DownloadMedia(string imgName, string formattedName)
    {
        ArgumentNullException.ThrowIfNull(imgName, nameof(imgName));
        ArgumentNullException.ThrowIfNull(formattedName, nameof(formattedName));
        try
        {
            using var stream = await mediaApi.GetFile(imgName);
            if (stream is null)
            {
                logger.Error(imgName + " not found or error occurred."); // 可能是文件不存在或其他错误
                return;
            }

            var filePath = formatter.GetFullPath(formattedName);
            if (File.Exists(filePath))
            {
                logger.Information($"{imgName} exits @ {filePath}");
                return;
            }

            using var outfile = File.Create(filePath);
            await stream.CopyToAsync(outfile);

            logger.Information($"Downloaded {imgName} to {filePath}");
        }
        catch (Exception ex)
        {
            logger.Error($"Error downloading media {imgName}: {ex.Message}");
            return;
        }
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
