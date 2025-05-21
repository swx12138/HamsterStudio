using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.DataModels;

public class InteractInfoModel
{
    [JsonPropertyName("collected")]
    public bool Collected { get; set; }

    [JsonPropertyName("collectedCount")]
    public string CollectedCount { get; set; } = string.Empty;

    [JsonPropertyName("commentCount")]
    public string CommentCount { get; set; } = string.Empty;

    [JsonPropertyName("followed")]
    public bool Followed { get; set; }

    [JsonPropertyName("liked")]
    public bool Liked { get; set; }

    [JsonPropertyName("likedCount")]
    public string LikedCount { get; set; } = string.Empty;

}

public class TagModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class ImageListItemModel
{
    [JsonPropertyName("livePhoto")]
    public bool LivePhoto { get; set; }

    [JsonPropertyName("urlDefault")]
    public string DefaultUrl { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("stream")]
    public StreamModel Stream { get; set; } = new();
}

public class ConsumerModel
{
    [JsonPropertyName("originVideoKey")]
    public string OriginVideoKey { get; set; } = string.Empty;
}

public class VideoInfoModel
{
    [JsonPropertyName("consumer")]
    public ConsumerModel Consumer { get; set; } = new();

}

public class StreamInfoModel
{
    [JsonPropertyName("masterUrl")]
    public string MasterUrl { get; set; } = string.Empty;

    [JsonPropertyName("backupUrls")]
    public List<string> BackupUrls { get; set; } = [];
}

public class StreamModel
{
    [JsonPropertyName("h264")]
    public StreamInfoModel[] H264 { get; set; } = [];

    [JsonPropertyName("h265")]
    public StreamInfoModel[] H265 { get; set; } = [];

    [JsonPropertyName("h266")]
    public StreamInfoModel[] H266 { get; set; } = [];

    [JsonPropertyName("av1")]
    public StreamInfoModel[] Av1 { get; set; } = [];
}

public class UserInfoModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = string.Empty;

    [JsonPropertyName("xsecToken")]
    public string XSecToken { get; set; } = string.Empty;
}

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

public class NoteDetailMapModel
{
    [JsonPropertyName("currentTime")]
    public long CurrentTime { get; set; }

    [JsonPropertyName("note")]
    public NoteDetailModel NoteDetail { get; set; } = new();
}

public class ServerRequestInfoModel
{
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("errMsg")]
    public string ErrorMessage { get; set; } = string.Empty;
}

public class NoteDataModel
{
    [JsonPropertyName("firstNoteId")]
    public string FirstNoteId { get; set; } = string.Empty;

    [JsonPropertyName("currentNoteId")]
    public string CurrentNoteId { get; set; } = string.Empty;

    [JsonPropertyName("noteDetailMap")]
    public Dictionary<string, NoteDetailMapModel> NoteDetailMap { get; set; } = [];

    [JsonPropertyName("serverRequestInfo")]
    public ServerRequestInfoModel ServerRequestInfo { get; set; } = new();
}
