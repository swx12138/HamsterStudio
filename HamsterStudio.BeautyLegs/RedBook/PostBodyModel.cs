using System.Text.Json.Serialization;

namespace HamsterStudio.BeautyLegs.RedBook;

public class PostBodyModel
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("download")]
    public bool Download { get; set; } = false;

}
