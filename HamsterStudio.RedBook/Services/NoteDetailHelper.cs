using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.RedBook.Models.Sub;

namespace HamsterStudio.RedBook.Services;

internal static class NoteDetailHelper
{
    public static string SelectTitle(NoteDetailModel noteDetail) =>
!string.IsNullOrEmpty(noteDetail.Title) ? noteDetail.Title : noteDetail.Time.ToString();

    public static string GetTypeName(NoteDetailModel noteDetail) =>
        noteDetail.Type switch { "normal" => "图文", "video" => "视频", _ => "Unknown" };

    public static StreamInfoModel SelectStream(StreamModel stream) =>
        stream.H266?.FirstOrDefault() ?? stream.H265?.FirstOrDefault() ??
        stream.H264?.FirstOrDefault() ?? stream.Av1?.FirstOrDefault() ??
        throw new ArgumentException("No valid stream");

    public static Uri GeneratePngLink(string token)
    {
        return new Uri($"https://ci.xiaohongshu.com/{token}?imageView2/format/png");
    }

    public static Uri GenerateWebpLink(string token)
    {
        return new Uri($"https://sns-img-bd.xhscdn.com/{token}");
    }

    public static Uri GenerateVideoLink(string token)
    {
        string uri = $"https://sns-video-bd.xhscdn.com/{token}";
        Logger.Shared.Information("视频链接：" + uri) ;
        return new Uri(uri);
    }

    public static string ExtractToken(string url)
    {
        string token = url.Split("!").First();
        token = token.Substring(token.IndexOf5th('/') + 1);
        return token;
    }
}
