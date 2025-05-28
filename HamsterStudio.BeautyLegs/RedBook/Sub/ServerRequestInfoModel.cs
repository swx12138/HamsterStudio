using System.Text.Json.Serialization;

namespace HamsterStudio.BeautyLegs.RedBook.Sub;

public class ServerRequestInfoModel
{
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("errMsg")]
    public string ErrorMessage { get; set; } = string.Empty;
}
