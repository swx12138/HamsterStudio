using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models.Sub;

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
