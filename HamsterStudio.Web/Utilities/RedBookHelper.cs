using HamsterStudio.Barefeet.Extensions;
using HamsterStudio.Barefeet.Logging;
using HamsterStudio.Web.DataModels.ReadBook;
using HamsterStudio.Web.Interfaces;
using System;
using System.Text.Json;

namespace HamsterStudio.Web.Utilities;

public static class RedBookHelper
{
    public static StreamInfoModel SelectStream(StreamModel stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.H266 != null && stream.H266.Length > 0) { return stream.H266[0]; }
        else if (stream.H265 != null && stream.H265.Length > 0) { return stream.H265[0]; }
        else if (stream.H264 != null && stream.H264.Length > 0) { return stream.H264[0]; }
        else if (stream.Av1 != null && stream.Av1.Length > 0) { return stream.Av1[0]; }

        throw new ArgumentException("No vaild stream", nameof(stream));
    }

    public static string SelectTitle(NoteDetailModel noteDetail)
    {
        if (noteDetail.Title != string.Empty)
        {
            return noteDetail.Title;
        }
        return noteDetail.Time.ToString();
    }

    public static string GetTypeName(NoteDetailModel noteDetail)
    {
        return noteDetail.Type switch
        {
            "normal" => "图文",
            "video" => "视频",
            _ => "Unkonwn"
        };
    }

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

    public static void DumpJson(string path, NoteDataModel noteData)
    {
        try
        {
            string text = JsonSerializer.Serialize(noteData);
            File.WriteAllText(path, text);
        }
        catch (Exception ex)
        {
            Logger.Shared.Warning($"Dump json failed.{ex.Message}\n{ex.StackTrace}");
        }
    }

    public static string GeneratePngLink(string baseUrl)
    {
        string token = baseUrl.Split("!").First();
        token = token.Substring(token.IndexOf5th('/') + 1);
        return $"https://ci.xiaohongshu.com/{token}?imageView2/format/png";
    }

    public static string GenerateWebpLink(string baseUrl)
    {
        string token = baseUrl.Split("!").First();
        token = token.Substring(token.IndexOf5th('/') + 1);
        return $"https://sns-img-bd.xhscdn.com/{token}";
    }

    public static string GenerateVideoLink(string token)
    {
        //return $"https://sns-img-bd.xhscdn.com/{token}";
        return $"https://sns-video-bd.xhscdn.com/{token}";
    }
}
