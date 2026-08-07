using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public class LevelInfoModel
    {
        [JsonPropertyName("current_exp")]
        public long CurrentExp { get; set; } = 0;

        [JsonPropertyName("current_level")]
        public long CurrentLevel { get; set; }

        [JsonPropertyName("current_min")]
        public long CurrentMin { get; set; } = 0;

        [JsonPropertyName("next_exp")]
        public string NextExp { get; set; } = string.Empty;
    }
}
