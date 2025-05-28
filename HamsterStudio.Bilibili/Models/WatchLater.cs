using HamsterStudio.Bilibili.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models
{
    public class WatchLaterData
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("count")]
        public long Count { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("list")]
        public List<WatchLaterDat> List { get; set; } = [];
    }
}