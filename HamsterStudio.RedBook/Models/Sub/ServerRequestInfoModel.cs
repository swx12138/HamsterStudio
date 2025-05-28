using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models.Sub;

public class ServerRequestInfoModel
{
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("errMsg")]
    public string ErrorMessage { get; set; } = string.Empty;
}
