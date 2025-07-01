using HamsterStudio.Bilibili.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models;

public class VideoStreamInfo
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("result")]
    public string Result { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("quality")]
    public long Quality { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("timelength")]
    public long Timelength { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("accept_format")]
    public string AcceptFormat { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("accept_description")]
    public List<string> AcceptDescription { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("accept_quality")]
    public List<int> AcceptQuality { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("video_codecid")]
    public long VideoCodecid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("seek_param")]
    public string SeekParam { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("seek_type")]
    public string SeekType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("dash")]
    public Dash Dash { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("support_formats")]
    public List<SupportFormatsItem> SupportFormats { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("high_format")]
    public string HighFormat { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("last_play_time")]
    public long LastPlayTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("last_play_cid")]
    public long LastPlayCid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("view_info")]
    public string ViewInfo { get; set; }

    [JsonPropertyName("durl")]
    public DurlItemModel[] Durl { get; set; } = [];
}

public class DurlItemModel
{
    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("ahead")]
    public string Ahead { get; set; } = string.Empty;

    [JsonPropertyName("vhead")]
    public string Vhead { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("backup_url")]
    public string[] BackupUrl { get; set; } = [];
}

