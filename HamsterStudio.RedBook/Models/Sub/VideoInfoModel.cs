using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models.Sub;

public class VideoInfoModel
{
    [JsonPropertyName("consumer")]
    public ConsumerModel Consumer { get; set; } = new();

}
