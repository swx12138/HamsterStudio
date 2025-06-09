using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels;

public class ServerRespData
{
    [JsonPropertyName("作品标题")]
    public string Title { get; set; }

    [JsonPropertyName("作品描述")]
    public string Description { get; set; }

    [JsonPropertyName("作者昵称")]
    public string AuthorNickName { get; set; }

    [JsonPropertyName("static_files")]
    public string[] StaticFiles { get; set; }

}

public class ServerRespModel
{
    [JsonPropertyName("messge")]
    public string Message { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public ServerRespData Data { get; set; }


}
