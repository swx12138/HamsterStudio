using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;
using System.Text.Json.Serialization;

namespace HamsterStudio.BraveShine.Models.Bilibili
{
    public struct VideoStreamInfo
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("from")]
        public string From { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("result")]
        public string Result { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

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
        /// 
        /// </summary>
        [JsonPropertyName("timelength")]
        public long Timelength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("accept_format")]
        public string AcceptFormat { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("accept_description")]
        public List<string> AcceptDescription { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("accept_quality")]
        public List<int> AcceptQuality { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("video_codecid")]
        public long VideoCodecid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("seek_param")]
        public string SeekParam { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("seek_type")]
        public string SeekType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("dash")]
        public Dash Dash { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("support_formats")]
        public List<SupportFormatsItem> SupportFormats { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("high_format")]
        public string HighFormat { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("last_play_time")]
        public long LastPlayTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("last_play_cid")]
        public long LastPlayCid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("view_info")]
        public string ViewInfo { get; set; }

    }
}