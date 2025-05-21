using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.DataModels
{
    public class WebFetchedData
    {
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("cover")]
        public string Cover { get; set; } = string.Empty;

    }
}
