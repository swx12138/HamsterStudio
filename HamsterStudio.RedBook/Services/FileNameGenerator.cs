using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.RedBook.DataModels;

namespace HamsterStudio.RedBook.Services;

public static class FileNameGenerator
{
    public static string GenerateFilename(string tiltle, int? index, UserInfoModel userInfo, string token)
    {
        return FileNameUtil.SanitizeFileName($"{tiltle}_{index}_xhs_{userInfo.Nickname}_{token}.png");
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

    public static (string png, string webp) GenerateImageFilenames(string title, int index, UserInfoModel user, ImageListItemModel imgInfo)
    {
        string token = imgInfo.DefaultUrl.Split('!').First().Split('/').Last();
        string filename = FileNameGenerator.GenerateFilename(title, index, user, token);
        return (filename, filename.Replace(".png", ".webp"));
    }
}
