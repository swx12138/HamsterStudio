using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models;

public class ConclusionModel
{
    [JsonPropertyName("code")]
    public int code { get; set; }

    [JsonPropertyName("model_result")]
    public ModelResultModel ModelResult { get; set; } = new();

    [JsonPropertyName("stid")]
    public string Stid { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("like_num")]
    public int LikeNum { get; set; }

    [JsonPropertyName("dislike_num")]
    public int DislikeNum { get; set; }

}

public class ModelResultModel
{
    [JsonPropertyName("result_type")]
    public int ResultType { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("outline")]
    public OutlineModel[] Outline { get; set; } = [];

}

public class OutlineModel
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("part_outline")]
    public OutlinePartModel[] PartOutline { get; set; } = [];

    [JsonPropertyName("timestamp")]
    public ulong Timestamp { get; set; }
}

public class OutlinePartModel
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public ulong Timestamp { get; set; }
}

public class BiliTicketModel
{

    [JsonPropertyName("created_at")]
    public ulong CreateAt { get; set; }

    [JsonPropertyName("ttl")]
    public int TTL { get; set; }

    [JsonPropertyName("nav")]
    public NavModel Nav { get; set; } = new();
}

public class NavModel
{
     [JsonPropertyName("img")]
    public string Img { get; set; } = string.Empty;

     [JsonPropertyName("sub")]
    public string Sub { get; set; } = string.Empty;

}
