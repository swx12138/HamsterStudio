using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models.Sub;

public class ConsumerModel
{
    [JsonPropertyName("originVideoKey")]
    public string OriginVideoKey { get; set; } = string.Empty;
}
