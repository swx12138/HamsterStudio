using HamsterStudio.Barefeet.FileSystem;
using HamsterStudio.Douyin.DataModels;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.FileSystem;
using HamsterStudio.Web.Services;

namespace HamsterStudio.Douyin.Services;

public class DouyinResourcesDownloadService(CommonDownloader downloader, FileCountGroupr groupr)
{
    private DownloadFileMgmt FileMgmt { get; } = new DownloadFileMgmt(@"E:\HamsterStudioHome\Douyin", groupr);
    public async Task<ServerRespModel> DownloadResourcesBy(RequestDataModel request)
    {
        var groupName = request.UserName;
        groupr.UpdateFileCount(groupName, request.ResourceUrls.Count);
        var list = new List<string>();
        for (int i = 0; i < request.ResourceUrls.Count; i++)
        {
            string name = FileNamingTools.GetFilenameFromUrl(request.ResourceUrls[i]);
            string filename = FileNamingTools.FormatFilename(name, "douyin", request.Title, request.UserName);
            string dest = FileMgmt.CreateFile(filename, request.UserName);
            if (await downloader.EasyDownloadFileAsync(new Uri(request.ResourceUrls[i]), dest))
            {
                list.Add(dest);
            }
        }
        return new ServerRespModel()
        {
            Status = 0,
            Message = "Succeed",
            Data = new ServerRespData()
            {
                AuthorNickName = request.UserName,
                Description = request.Description,
                Title = request.Title,
                StaticFiles = []
            }
        };
    }
}
