using HamsterStudio.Bilibili.Models.Sub;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models
{
    public class VideoInfo
    {
        [JsonPropertyName("bvid")]
        [Category("Basic")]
        [Editable(false)]
        public string Bvid { get; set; } = string.Empty;

        [JsonPropertyName("aid")]
        [Browsable(false)]
        public long Aid { get; set; }

        [JsonPropertyName("pic")]
        [Editable(false)]
        public string Pic { get; set; } = string.Empty;

        /// <summary>
        /// 谢谢你帮我捡鞋子(◜𖥦◝ ）
        /// </summary>
        [JsonPropertyName("title")]
        [Category("Basic")]
        [Editable(false)]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        [Category("Basic")]
        [Editable(false)]
        public string Desc { get; set; } = string.Empty;

        [JsonPropertyName("desc_v2")]
        [Editable(false)]
        public List<DescV2Item> DescV2 { get; set; } = [];

        [JsonPropertyName("state")]
        [Editable(false)]
        public long State { get; set; }

        [JsonPropertyName("owner")]
        [Editable(false)]
        public Owner Owner { get; set; }

        [JsonPropertyName("stat")]
        public Stat Stat { get; set; }

        [JsonPropertyName("cid")]
        [Editable(false)]
        public long Cid { get; set; }

        [JsonPropertyName("pages")]
        [Browsable(false)]
        public List<PagesItem> Pages { get; set; } = [];

    }
}
