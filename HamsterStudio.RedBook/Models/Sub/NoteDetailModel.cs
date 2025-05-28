using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models.Sub;

public class NoteDetailModel
{
    [JsonPropertyName("xsecToken")]
    public string XSecToken { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("interactInfo")]
    public InteractInfoModel InteractInfo { get; set; } = new();

    [JsonPropertyName("imageList")]
    public List<ImageListItemModel> ImageList { get; set; } = [];

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("lastUpdateTime")]
    public long LastUpdateTime { get; set; }

    [JsonPropertyName("ipLocation")]
    public string IpLocation { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public UserInfoModel UserInfo { get; set; } = new();

    [JsonPropertyName("tagList")]
    public List<TagModel> TagList { get; set; } = [];

    [JsonPropertyName("video")]
    public VideoInfoModel VideoInfo { get; set; } = new();
}
