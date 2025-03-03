using HamsterStudio.Web.DataModels.Bilibili.SubStruct;
using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili
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