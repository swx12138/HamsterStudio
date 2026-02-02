using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Services;
using HamsterStudio.Barefeet.SysCall;
using HamsterStudio.Web.DataModels;
using HamsterStudio.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace HamsterStudio.WebApi.Controllers;

public class MihoyoCommunityDownloadRequest
{
    [JsonPropertyName("urls")]
    public string[] Urls { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    public string Hash()
    {
        return HashCode.Combine(string.Join("_", Id, UserId)).ToString();
    }

    public string FormatFilenmae(int idx)
    {
        if (idx >= Urls.Length)
        {
            return string.Empty;
        }
        return FormatFilenmae(Title.ValueOr(Hash()), Name, Id, UserId, Urls[idx], idx);
    }

    public static string FormatFilenmae(string title, string userName, string uid, string postId, string url, int idx)
    {
        // #原神 #cos正片 ＃八重神子_2_薛定谔の幼猫_mys_39034740_8930248_b5c7b64e78eacd3dac97781879eadb8f_7725792306379522560.jpg
        var filename = url.Split('?').First().Split('@').First().Split('/').Last();
        return FileNameUtil.SanitizeFileName($"{title}_{idx}_{userName}_mys_{uid}_{postId}_{filename}");
    }
}


[ApiController]
[Route("/miyoushe")]
public class MihoyoCommunityController(CommonDownloader downloader, DirectoryMgmt directoryMgmt, ILogger<MihoyoCommunityController> logger) : ControllerBase
{
    private DirectoryInfo _directory = new(Path.Combine(directoryMgmt.StorageHome, "miyoushe"));

    [HttpPost("download-web-pc")]
    public async Task<ServerRespModel> Download_Web_PC([FromBody] MihoyoCommunityDownloadRequest request)
    {
        List<string> downloaded = [];
        for (int iter = 0; iter < request.Urls.Length; iter++)
        {
            var url = request.Urls[iter];

            var destPath = Path.Combine(_directory.FullName, request.Name);
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);

                string cd = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(_directory.FullName);
                ShellApi.System($"move *_{request.Name}_mys_* {request.Name}");
                Directory.SetCurrentDirectory(cd);
            }

            var fullname = Path.Combine(destPath, request.FormatFilenmae(iter));
            _ = await downloader.EasyDownloadFileAsync(new Uri(request.Urls[iter]), fullname);
            downloaded.Add(fullname);
        }

        return new ServerRespModel
        {
            Message = "ok",
            Data = new ServerRespData
            {
                Title = request.Title,
                Description = $"Downloaded {downloaded.Count} files.",
                AuthorNickName = request.Name,
                StaticFiles = [.. downloaded
                    .Select(f => Path.GetRelativePath(_directory.FullName, f))
                    .Select(f=> f.Replace('\\','/'))
                    .OrderBy(f => f,        new NaturalStringComparer())]
            }
        };

    }
}
