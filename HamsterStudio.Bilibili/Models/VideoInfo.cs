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

        [JsonPropertyName("videos")]
        [Browsable(false)]
        public long Videos { get; set; }

        [JsonPropertyName("tid")]
        public long Tid { get; set; }

        [JsonPropertyName("tname")]
        [Category("Basic")]
        [Editable(false)]
        public string Tname { get; set; } = string.Empty;

        [JsonPropertyName("copyright")]
        [Category("Basic")]
        [Editable(false)]
        public long Copyright { get; set; }

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

        [JsonPropertyName("pubdate")]
        [Editable(false)]
        public long Pubdate { get; set; }

        [JsonPropertyName("ctime")]
        [Editable(false)]
        public long Ctime { get; set; }

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

        [JsonPropertyName("duration")]
        [Editable(false)]
        public long Duration { get; set; }

        [JsonPropertyName("mission_id")]
        [Editable(false)]
        public long MissionId { get; set; }

        [JsonPropertyName("rights")]
        [Browsable(false)]
        public Rights Rights { get; set; }

        [JsonPropertyName("owner")]
        [Editable(false)]
        public Owner Owner { get; set; }

        [JsonPropertyName("stat")]
        public Stat Stat { get; set; }

        [JsonPropertyName("argue_info")]
        [Browsable(false)]
        public ArgueInfo ArgueInfo { get; set; }

        [JsonPropertyName("dynamic")]
        [Category("Basic")]
        [Editable(false)]
        public string Dynamic { get; set; } = string.Empty;

        [JsonPropertyName("cid")]
        [Editable(false)]
        public long Cid { get; set; }

        [JsonPropertyName("dimension")]
        [Browsable(false)]
        public Dimension Dimension { get; set; }

        [JsonPropertyName("premiere")]
        [Editable(false)]
        public string Premiere { get; set; } = string.Empty;

        [JsonPropertyName("teenage_mode")]
        [Editable(false)]
        public long TeenageMode { get; set; }

        [JsonPropertyName("is_chargeable_season")]
        [Editable(false)]
        public bool IsChargeableSeason { get; set; }

        [JsonPropertyName("is_story")]
        [Editable(false)]
        public bool IsStory { get; set; }

        [JsonPropertyName("is_upower_exclusive")]
        [Editable(false)]
        public bool IsUpowerExclusive { get; set; }

        [JsonPropertyName("is_upower_play")]
        [Editable(false)]
        public bool IsUpowerPlay { get; set; }

        [JsonPropertyName("is_upower_preview")]
        [Editable(false)]
        public bool IsUpowerPreview { get; set; }

        [JsonPropertyName("enable_vt")]
        [Editable(false)]
        public long EnableVt { get; set; }

        [JsonPropertyName("vt_display")]
        [Editable(false)]
        public string VtDisplay { get; set; } = string.Empty;

        [JsonPropertyName("no_cache")]
        [Editable(false)]
        public bool NoCache { get; set; }

        [JsonPropertyName("pages")]
        [Browsable(false)]
        public List<PagesItem> Pages { get; set; } = [];

        [JsonPropertyName("subtitle")]
        [Editable(false)]
        public Subtitle Subtitle { get; set; }

        [JsonPropertyName("is_season_display")]
        [Editable(false)]
        public bool IsSeasonDisplay { get; set; }

        [JsonPropertyName("user_garb")]
        [Editable(false)]
        public UserGarb UserGarb { get; set; }

        [JsonPropertyName("honor_reply")]
        [Editable(false)]
        public HonorReply HonorReply { get; set; }

        [JsonPropertyName("like_icon")]
        [Editable(false)]
        public string LikeIcon { get; set; } = string.Empty;

        [JsonPropertyName("need_jump_bv")]
        [Editable(false)]
        public bool NeedJumpBv { get; set; }

        [JsonPropertyName("disable_show_up_info")]
        [Editable(false)]
        public bool DisableShowUpInfo { get; set; }

        [JsonPropertyName("is_story_play")]
        [Editable(false)]
        public long IsStoryPlay { get; set; }
    }
}
