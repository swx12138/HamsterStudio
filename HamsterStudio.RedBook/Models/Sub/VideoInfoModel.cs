using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models.Sub;

public class VideoInfoMediaModel {
    [JsonPropertyName("stream")]
    public MediaStreamModel Stream { get; set; } = new();
}

public class MediaStreamListItemModel
{
    [JsonPropertyName("audioBitrate")]
    public long AudioBitrate { get; set; }

    [JsonPropertyName("videoBitrate")]
    public long VideoBitrate { get; set; }

    [JsonPropertyName("backupUrls")]
    public string[] BackupUrls { get; set; } = [];

    [JsonPropertyName("masterUrl")]
    public string MasterUrl { get; set; } = string.Empty;

}

public class MediaStreamModel
{
    [JsonPropertyName("h266")]
    public MediaStreamListItemModel[] H266List { get; set; } = [];

    [JsonPropertyName("h265")]
    public MediaStreamListItemModel[] H265List { get; set; } = [];

    [JsonPropertyName("h264")]
    public MediaStreamListItemModel[] H264List { get; set; } = [];

    [JsonPropertyName("av1")]
    public MediaStreamListItemModel[] Av1List { get; set; } = [];

}

public class VideoInfoModel
{
    [JsonPropertyName("consumer")]
    public ConsumerModel Consumer { get; set; } = new();

    [JsonPropertyName("media")]
    public VideoInfoMediaModel Media { get; set; } = new();

}
