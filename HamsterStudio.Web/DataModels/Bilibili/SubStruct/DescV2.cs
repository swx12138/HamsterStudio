using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili.SubStruct
{
    public struct DescV2Item
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("raw_text")]
        public string RawText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("type")]
        public long Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("biz_id")]
        public long BizId { get; set; }
    }
}