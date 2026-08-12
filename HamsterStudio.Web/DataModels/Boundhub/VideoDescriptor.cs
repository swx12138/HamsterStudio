using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Boundhub
{
    internal class VideoDescriptor
    {
        [JsonPropertyName("url")]
        public string Uri { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("cookie")]
        public string Cookie { get; set; } = string.Empty;

        [JsonPropertyName("refer")]
        public string OriginUrl { get; set; } = string.Empty;
    }
}
