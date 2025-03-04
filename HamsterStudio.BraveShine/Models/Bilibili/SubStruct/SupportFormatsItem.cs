using System.Text.Json.Serialization;

namespace HamsterStudio.BraveShine.Models.Bilibili.SubStruct
{
    public struct SupportFormatsItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("quality")]
        public long Quality { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("format")]
        public string Format { get; set; }

        /// <summary>
        /// 4K 超清
        /// </summary>
        [JsonPropertyName("new_description")]
        public string NewDescription { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("display_desc")]
        public string DisplayDesc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("superscript")]
        public string Superscript { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("codecs")]
        public List<string> Codecs { get; set; }
    }
}