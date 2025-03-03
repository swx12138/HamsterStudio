using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili
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
