using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models.Sub;

public class UserInfoModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = string.Empty;

    [JsonPropertyName("xsecToken")]
    public string XSecToken { get; set; } = string.Empty;
}
