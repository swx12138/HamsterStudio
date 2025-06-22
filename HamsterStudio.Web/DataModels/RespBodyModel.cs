using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels;

public record ServerRespData
{
    [JsonPropertyName("作品标题")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("作品描述")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("作者昵称")]
    public string AuthorNickName { get; init; } = string.Empty;

    [JsonPropertyName("static_files")]
    public string[] StaticFiles { get; init; } = [];

}

public class ServerRespModel
{
    [JsonPropertyName("messge")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; } 

    [JsonPropertyName("data")]
    public ServerRespData Data { get; init; } = new();

}
