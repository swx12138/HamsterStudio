using System.Text.Json.Serialization;

namespace HamsterStudio.BeautyLegs.RedBook.Sub
{
    public class VideoInfoModel
    {
        [JsonPropertyName("consumer")]
        public ConsumerModel Consumer { get; set; } = new();

    }
}
