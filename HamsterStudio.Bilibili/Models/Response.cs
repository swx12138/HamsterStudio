using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models
{
    public class Response<T>
    {
        [JsonPropertyName("code")]
        public long Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("ttl")]
        public long TTL { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

}
