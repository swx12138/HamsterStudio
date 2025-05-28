using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public struct Dimension
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("width")]
        public long Width { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("height")]
        public long Height { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("rotate")]
        public long Rotate { get; set; }
    }
}