using System.Text.Json.Serialization;

namespace HamsterStudio.BeautyLegs.RedBook.Sub
{
    public class ConsumerModel
    {
        [JsonPropertyName("originVideoKey")]
        public string OriginVideoKey { get; set; } = string.Empty;
    }
}
