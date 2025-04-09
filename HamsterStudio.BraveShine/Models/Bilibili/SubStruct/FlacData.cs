using System.Text.Json.Serialization;

namespace HamsterStudio.BraveShine.Models.Bilibili.SubStruct
{
    public class FlacData
    {
        [JsonPropertyName("display")]
        public bool Display { get; set; }

        [JsonPropertyName("audio")]
        public DashAvInfo Audio { get; set; }

    }
}
