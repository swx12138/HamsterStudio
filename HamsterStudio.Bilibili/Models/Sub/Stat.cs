using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public struct Stat
    {
        [JsonPropertyName("aid")]
        public long Aid { get; set; }

        [JsonPropertyName("view")]
        [Category("Basic")]
        public long View { get; set; }

        [JsonPropertyName("danmaku")]
        [Category("Comments")]
        public long Danmaku { get; set; }

        [JsonPropertyName("reply")]
        [Category("Comments")]
        public long Reply { get; set; }

        [JsonPropertyName("favorite")]
        [Category("Basic")]
        public long Favorite { get; set; }

        [JsonPropertyName("coin")]
        [Category("Basic")]
        public long Coin { get; set; }

        [JsonPropertyName("share")]
        [Category("Comments")]
        public long Share { get; set; }

        [JsonPropertyName("now_rank")]
        public long NowRank { get; set; }

        [JsonPropertyName("his_rank")]
        public long HisRank { get; set; }

        [JsonPropertyName("like")]
        [Category("Basic")]
        public long Like { get; set; }

        [JsonPropertyName("dislike")]
        public long Dislike { get; set; }

        [JsonPropertyName("evaluation")]
        public string Evaluation { get; set; }

        [JsonPropertyName("vt")]
        public long Vt { get; set; }
    }
}