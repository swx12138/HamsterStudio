using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public struct PagesItem
    {
        [JsonPropertyName("cid")]
        public long Cid { get; set; }

        [JsonPropertyName("page")]
        public long Page { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        /// <summary>
        /// 谢谢你帮我捡鞋子(◜𖥦◝ ）
        /// </summary>
        [JsonPropertyName("part")]
        public string Title { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("vid")]
        public string Vid { get; set; }

        [JsonPropertyName("weblink")]
        public string Weblink { get; set; }

        [JsonPropertyName("dimension")]
        public Dimension Dimension { get; set; }

        [JsonPropertyName("first_frame")]
        public string FirstFrame { get; set; }

    }
}