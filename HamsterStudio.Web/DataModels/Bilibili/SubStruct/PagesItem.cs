using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili.SubStruct
{
    public struct PagesItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cid")]
        public long Cid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("page")]
        public long Page { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("from")]
        public string From { get; set; }

        /// <summary>
        /// 谢谢你帮我捡鞋子(◜𖥦◝ ）
        /// </summary>
        [JsonPropertyName("part")]
        public string Part { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("vid")]
        public string Vid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("weblink")]
        public string Weblink { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("dimension")]
        public Dimension Dimension { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("first_frame")]
        public string FirstFrame { get; set; }

    }
}