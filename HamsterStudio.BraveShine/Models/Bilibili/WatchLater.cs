using HamsterStudio.BraveShine.Modelss.Bilibili.SubStruct;
using System.Text.Json.Serialization;

namespace HamsterStudio.BraveShine.Models.Bilibili
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
        public List<WatchLaterDat> List { get; set; }
    }
}