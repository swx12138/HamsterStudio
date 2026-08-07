using HamsterStudio.Bilibili.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models;

public class VideoStreamInfo
{
    [JsonPropertyName("accept_format")]
    public string AcceptFormat { get; set; }

    [JsonPropertyName("accept_description")]
    public List<string> AcceptDescription { get; set; }

    [JsonPropertyName("accept_quality")]
    public List<int> AcceptQuality { get; set; }

    [JsonPropertyName("dash")]
    public Dash Dash { get; set; }

    [JsonPropertyName("support_formats")]
    public List<SupportFormatsItem> SupportFormats { get; set; }

    [JsonPropertyName("durl")]
    public DurlItemModel[] Durl { get; set; } = [];
}

public class DurlItemModel
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("backup_url")]
    public string[] BackupUrl { get; set; } = [];
}

