using System.Text.Json.Serialization;

namespace HamsterStudio.BeautyLegs.RedBook.Sub
{
    public class StreamModel
    {
        [JsonPropertyName("h264")]
        public StreamInfoModel[] H264 { get; set; } = [];

        [JsonPropertyName("h265")]
        public StreamInfoModel[] H265 { get; set; } = [];

        [JsonPropertyName("h266")]
        public StreamInfoModel[] H266 { get; set; } = [];

        [JsonPropertyName("av1")]
        public StreamInfoModel[] Av1 { get; set; } = [];
    }
}
