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
    public string Outline {  get; set; } = string.Empty;

}
