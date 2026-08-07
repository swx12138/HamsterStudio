using HamsterStudio.Bilibili.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models
{
    public class NavDataModel
    {
        [JsonPropertyName("uname")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("mid")]
        public long UserId { get; set; } = 0;

        [JsonPropertyName("face")]
        public string UserAvatar { get; set; } = string.Empty;

        [JsonPropertyName("level_info")]
        public LevelInfoModel LevelInfo { get; set; } = new LevelInfoModel();

        [JsonPropertyName("money")]
        public double Money { get; set; } = 0.0;
    }
}
