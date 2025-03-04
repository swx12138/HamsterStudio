using System.Text.Json.Serialization;

namespace HamsterStudio.BraveShine.Models.Bilibili
{
    public struct Response<T>
    {
        [JsonPropertyName("code")]
        public long Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("ttl")]
        public long TTL { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

}
