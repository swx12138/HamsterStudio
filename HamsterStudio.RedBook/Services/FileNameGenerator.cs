using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.RedBook.Models.Sub;

namespace HamsterStudio.RedBook.Services;

public static class FileNameGenerator
{
    public static string GenerateImageFilename(string tiltle, int? index, UserInfoModel userInfo, string token)
    {
        string bare_token = token.Split('/').Last();
        return FileNameUtil.SanitizeFileName($"{tiltle}_{index}_xhs_{userInfo.Nickname}_{bare_token}");
    }

    public static string GenerateLivePhotoFilename(string tiltle, int? index, UserInfoModel userInfo, string streamUrl)
    {
        var rawname = streamUrl.Split('?').First().Split('/').Last();
        return FileNameUtil.SanitizeFileName($"{tiltle}_{index}_xhs_{userInfo.Nickname}_{rawname}");
    }

    public static string GenerateVideoFilename(string tiltle, UserInfoModel userInfo, string token)
    {
        // TBD:自动判断类型
        return FileNameUtil.SanitizeFileName($"{tiltle}_xhs_{userInfo.Nickname}_{token}.mp4");
    }

}
