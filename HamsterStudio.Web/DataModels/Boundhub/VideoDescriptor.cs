using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Boundhub
{
    internal class VideoDescriptor
    {
        [JsonPropertyName("url")]
        public string Uri { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("cookie")]
        public string Cookie { get; set; }

        [JsonPropertyName("refer")]
        public string OriginUrl { get; set; }
    }
}
