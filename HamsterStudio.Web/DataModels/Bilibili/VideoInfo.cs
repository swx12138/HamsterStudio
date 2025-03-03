using HamsterStudio.Web.DataModels.Bilibili.SubStruct;
using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili
{
    public class VideoInfo
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("bvid")]
        public string Bvid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("aid")]
        public long Aid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("videos")]
        public long Videos { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("tid")]
        public long Tid { get; set; }

        /// <summary>
        /// 仿妆cos
        /// </summary>
        [JsonPropertyName("tname")]
        public string Tname { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("copyright")]
        public long Copyright { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("pic")]
        public string Pic { get; set; }

        /// <summary>
        /// 谢谢你帮我捡鞋子(◜𖥦◝ ）
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("pubdate")]
        public long Pubdate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ctime")]
        public long Ctime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("desc_v2")]
        public List<DescV2Item> DescV2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("state")]
        public long State { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("mission_id")]
        public long MissionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("rights")]
        public Rights Rights { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("owner")]
        public Owner Owner { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("stat")]
        public Stat Stat { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("argue_info")]
        public ArgueInfo ArgueInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("dynamic")]
        public string Dynamic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cid")]
        public long Cid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("dimension")]
        public Dimension Dimension { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("premiere")]
        public string Premiere { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("teenage_mode")]
        public long TeenageMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_chargeable_season")]
        public bool IsChargeableSeason { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_story")]
        public bool IsStory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_upower_exclusive")]
        public bool IsUpowerExclusive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_upower_play")]
        public bool IsUpowerPlay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_upower_preview")]
        public bool IsUpowerPreview { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("enable_vt")]
        public long EnableVt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("vt_display")]
        public string VtDisplay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("no_cache")]
        public bool NoCache { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("pages")]
        public List<PagesItem> Pages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("subtitle")]
        public Subtitle Subtitle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_season_display")]
        public bool IsSeasonDisplay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("user_garb")]
        public UserGarb UserGarb { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("honor_reply")]
        public HonorReply HonorReply { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("like_icon")]
        public string LikeIcon { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("need_jump_bv")]
        public bool NeedJumpBv { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("disable_show_up_info")]
        public bool DisableShowUpInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_story_play")]
        public long IsStoryPlay { get; set; }
    }
}
