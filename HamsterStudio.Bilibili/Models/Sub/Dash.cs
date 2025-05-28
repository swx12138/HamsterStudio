using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public struct Dash
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("minBufferTime")]
        public double MinBufferTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("min_buffer_time")]
        public double MinBufferTime2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("video")]
        public List<DashAvInfo> Video { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("audio")]
        public List<DashAvInfo> Audio { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("dolby")]
        public Dolby Dolby { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("flac")]
        public FlacData Flac { get; set; }
    }
}