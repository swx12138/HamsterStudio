using HamsterStudio.BraveShine.Models.Bilibili.SubStruct;
using System.Text.Json.Serialization;

namespace HamsterStudio.BraveShine.Modelss.Bilibili.SubStruct
{
    public class WatchLaterDat
    {
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
        /// 主人晚上好
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
        /// 拍摄@照桥染柒
        /// </summary>
        [JsonPropertyName("desc")]
        public string Desc { get; set; }

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
        [JsonPropertyName("dynamic")]
        public string Dynamic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("dimension")]
        public Dimension Dimension { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("short_link_v2")]
        public string ShortLinkV2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("up_from_v2")]
        public long UpFromV2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("first_frame")]
        public string FirstFrame { get; set; }

        /// <summary>
        /// 江苏
        /// </summary>
        [JsonPropertyName("pub_location")]
        public string PubLocation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cover43")]
        public string Cover43 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("page")]
        public PagesItem Page { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("count")]
        public long Count { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("cid")]
        public long Cid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("progress")]
        public long Progress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("add_at")]
        public long AddAt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("bvid")]
        public string Bvid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("enable_vt")]
        public long EnableVt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("view_text_1")]
        public string ViewText1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("card_type")]
        public long CardType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("left_icon_type")]
        public long LeftIconType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("left_text")]
        public string LeftText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("right_icon_type")]
        public long RightIconType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("right_text")]
        public string RightText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("arc_state")]
        public long ArcState { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("pgc_label")]
        public string PgcLabel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("show_up")]
        public bool ShowUp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("forbid_fav")]
        public bool ForbidFav { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("forbid_sort")]
        public bool ForbidSort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("season_title")]
        public string SeasonTitle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("long_title")]
        public string LongTitle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("index_title")]
        public string IndexTitle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("c_source")]
        public string CSource { get; set; }
    }
}