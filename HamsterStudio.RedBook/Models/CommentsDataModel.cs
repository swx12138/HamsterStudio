using HamsterStudio.RedBook.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models;

public class CommentsDataModel
{
    [JsonPropertyName("list")]
    public CommentDataModel[] Comments { get; set; } = [];

    [JsonPropertyName("cursor")]
    public string Cursor { get; set; } = string.Empty;

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }

    [JsonPropertyName("loading")]
    public bool Loading { get; set; }

    [JsonPropertyName("firstRequestFinish")]
    public bool FirstRequestFinish { get; set; }
}

public class CommentDataModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("atUsers")]
    public UserInfoModel[] AtUsers { get; set; } = [];

    [JsonPropertyName("userInfo")]
    public UserInfoModel UserInfo { get; set; } = new();

    [JsonPropertyName("showTags")]
    public string[] ShowTags { get; set; } = [];

    [JsonPropertyName("createTime")]
    public long CreateTime { get; set; }

    [JsonPropertyName("liked")]
    public bool Liked { get; set; }

    [JsonPropertyName("subCommentCursor")]
    public string SubCommentCursor { get; set; } = string.Empty;

    [JsonPropertyName("pictures")]
    public PicturesItemModel[] Pictures { get; set; } = [];

    [JsonPropertyName("noteId")]
    public string NoteId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("subCommentCount")]
    public string SubCommentCount { get; set; } = string.Empty;

    [JsonPropertyName("subComments")]
    public CommentDataModel[] SubComments { get; set; } = [];

    [JsonPropertyName("subCommentHasMore")]
    public bool SubCommentHasMore { get; set; }

    [JsonPropertyName("likeCount")]
    public string LikeCount { get; set; } = string.Empty;

    [JsonPropertyName("ipLocation")]
    public string IpLocation { get; set; } = string.Empty;

    [JsonPropertyName("expended")]
    public bool Expended { get; set; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }
}

public class PicturesItemModel
{
    [JsonPropertyName("infoList")]
    public InfoListItemModel[] InfoList { get; set; } = [];

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("urlPre")]
    public string UrlPre { get; set; } = string.Empty;

    [JsonPropertyName("urlDefault")]
    public string UrlDefault { get; set; } = string.Empty;
}

public class InfoListItemModel
{
    [JsonPropertyName("imageScene")]
    public string ImageScene { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public class TargetCommentModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("userInfo")]
    public UserInfoModel UserInfo { get; set; } = new();
}
