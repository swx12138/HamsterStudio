using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili.SubStruct
{
    public struct Stat
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("aid")]
        public long Aid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("view")]
        public long View { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("danmaku")]
        public long Danmaku { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("reply")]
        public long Reply { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("favorite")]
        public long Favorite { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("coin")]
        public long Coin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("share")]
        public long Share { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("now_rank")]
        public long NowRank { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("his_rank")]
        public long HisRank { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("like")]
        public long Like { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("dislike")]
        public long Dislike { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("evaluation")]
        public string Evaluation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("vt")]
        public long Vt { get; set; }
    }
}