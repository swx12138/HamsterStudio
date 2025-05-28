using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public class FlacData
    {
        [JsonPropertyName("display")]
        public bool Display { get; set; }

        [JsonPropertyName("audio")]
        public DashAvInfo Audio { get; set; }

    }
}
