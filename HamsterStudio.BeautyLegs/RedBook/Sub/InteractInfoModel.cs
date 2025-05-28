using System.Text.Json.Serialization;

namespace HamsterStudio.BeautyLegs.RedBook.Sub;

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
