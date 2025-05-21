using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.DataModels;

public class PostBodyModel
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("download")]
    public bool Download { get; set; } = false;

}
