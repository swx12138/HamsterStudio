using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels;

public class FtpUserConfiguration
{
    public class User
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("home")]
        public string? Home { get; set; }
    }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("users")]
    public User[] Users { get; set; } = [];

}
