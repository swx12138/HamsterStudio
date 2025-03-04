using System.Text.Json.Serialization;

namespace HamsterStudio.BraveShine.Models.Bilibili.SubStruct
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