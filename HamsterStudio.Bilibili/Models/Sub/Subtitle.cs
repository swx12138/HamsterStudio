using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public struct Subtitle
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("allow_submit")]
        public bool AllowSubmit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("list")]
        public List<SubtitleListItem> List { get; set; }
    }
}