using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili.SubStruct
{
    public struct SubtitleListItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("lan")]
        public string Lan { get; set; }

        /// <summary>
        /// 中文（自动翻译）
        /// </summary>
        [JsonPropertyName("lan_doc")]
        public string LanDoc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_lock")]
        public bool IsLock { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("subtitle_url")]
        public string SubtitleUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("type")]
        public long Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("id_str")]
        public string IdStr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ai_type")]
        public long AiType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ai_status")]
        public long AiStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("author")]
        public Author Author { get; set; }
    }
}