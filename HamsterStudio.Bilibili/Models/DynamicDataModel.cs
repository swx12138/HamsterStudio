using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models;

public class DynamicDataModel
{
    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("items")]
    public string[] Items { get; set; } = [];

}

