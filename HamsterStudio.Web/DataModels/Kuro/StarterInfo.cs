using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Kuro
{
    internal struct AnimateBackground
    {
        [JsonPropertyName("durationInSecond")]
        public int DurationInSecond { get; set; }

        [JsonPropertyName("frameRate")]
        public int FrameRate { get; set; }

        [JsonPropertyName("md5")]
        public string MD5 { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public struct CdnInfo
    {
        public int K1 { get; set; }
        public int K2 { get; set; }
        public int P { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public struct ChangeLog
    {
        [JsonPropertyName("zh-Hans")]
        public string Message { get; set; }
    }

    public struct StarterDefault
    {
        [JsonPropertyName("cdnList")]
        public IReadOnlyCollection<CdnInfo> CdnList { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("changelog")]
        public ChangeLog Change { get; set; }

    }

    internal class StarterInfo
    {
        [JsonPropertyName("default")]
        public StarterDefault Default { get; set; }

        [JsonPropertyName("animateBackground")]
        public AnimateBackground AnimateBackground { get; set; }

    }

}
