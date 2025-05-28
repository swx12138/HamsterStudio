using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models;

public class ServerRespenseModel
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<string> Files { get; set; } = [];

}
