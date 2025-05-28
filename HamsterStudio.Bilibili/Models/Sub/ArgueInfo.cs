using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public struct ArgueInfo
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("argue_msg")]
        public string ArgueMsg { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("argue_type")]
        public long ArgueType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("argue_link")]
        public string ArgueLink { get; set; }
    }
}