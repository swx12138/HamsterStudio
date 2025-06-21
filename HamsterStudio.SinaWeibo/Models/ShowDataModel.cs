using HamsterStudio.Barefeet.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HamsterStudio.SinaWeibo.Models;

public class ShowDataModel
{
    [JsonPropertyName("visible")]
    public VisibleModel Visible { get; set; } = new();

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("idstr")]
    public string Idstr { get; set; } = string.Empty;

    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;

    [JsonPropertyName("mblogid")]
    public string MblogId { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public UserModel User { get; set; } = new();

    [JsonPropertyName("can_edit")]
    public bool CanEdit { get; set; }

    [JsonPropertyName("textLength")]
    public int TextLength { get; set; }

    [JsonPropertyName("annotations")]
    public AnnotationsItemModel[] Annotations { get; set; } = [];

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("favorited")]
    public bool Favorited { get; set; }

    [JsonPropertyName("rid")]
    public string Rid { get; set; } = string.Empty;

    [JsonPropertyName("cardid")]
    public string Cardid { get; set; } = string.Empty;

    [JsonPropertyName("pic_ids")]
    public string[] PicIds { get; set; } = [];

    [JsonPropertyName("pic_num")]
    public int PicNum { get; set; }

    [JsonPropertyName("pic_infos")]
    public Dictionary<string, PicInfosValueModel> PicInfos { get; set; } = [];

    [JsonPropertyName("is_paid")]
    public bool IsPaid { get; set; }

    [JsonPropertyName("pic_bg_new")]
    public string PicBgNew { get; set; } = string.Empty;

    [JsonPropertyName("mblog_vip_type")]
    public int MblogVipType { get; set; }

    [JsonPropertyName("number_display_strategy")]
    public NumberDisplayStrategyModel NumberDisplayStrategy { get; set; } = new();

    [JsonPropertyName("reposts_count")]
    public int RepostsCount { get; set; }

    [JsonPropertyName("comments_count")]
    public int CommentsCount { get; set; }

    [JsonPropertyName("attitudes_count")]
    public int AttitudesCount { get; set; }

    [JsonPropertyName("attitudes_status")]
    public int AttitudesStatus { get; set; }

    [JsonPropertyName("isLongText")]
    public bool IsLongText { get; set; }

    [JsonPropertyName("mlevel")]
    public int Mlevel { get; set; }

    [JsonPropertyName("content_auth")]
    public int ContentAuth { get; set; }

    [JsonPropertyName("is_show_bulletin")]
    public int IsShowBulletin { get; set; }

    [JsonPropertyName("comment_manage_info")]
    public CommentManageInfoModel CommentManageInfo { get; set; } = new();

    [JsonPropertyName("share_repost_type")]
    public int ShareRepostType { get; set; }

    [JsonPropertyName("topic_struct")]
    public TopicStructItemModel[] TopicStruct { get; set; } = [];

    [JsonPropertyName("url_struct")]
    public UrlStructItemModel[] UrlStruct { get; set; } = [];

    [JsonPropertyName("tag_struct")]
    public TagStructItemModel[] TagStruct { get; set; } = [];

    [JsonPropertyName("title")]
    public TitleModel Title { get; set; } = new();

    [JsonPropertyName("mblogtype")]
    public int Mblogtype { get; set; }

    [JsonPropertyName("showFeedRepost")]
    public bool ShowFeedRepost { get; set; }

    [JsonPropertyName("showFeedComment")]
    public bool ShowFeedComment { get; set; }

    [JsonPropertyName("pictureViewerSign")]
    public bool PictureViewerSign { get; set; }

    [JsonPropertyName("showPictureViewer")]
    public bool ShowPictureViewer { get; set; }

    [JsonPropertyName("rcList")]
    public object[] RcList { get; set; } = [];

    [JsonPropertyName("content_auth_list")]
    public ContentAuthListItemModel[] ContentAuthList { get; set; } = [];

    [JsonPropertyName("analysis_extra")]
    public string AnalysisExtra { get; set; } = string.Empty;

    [JsonPropertyName("readtimetype")]
    public string Readtimetype { get; set; } = string.Empty;

    [JsonPropertyName("mixed_count")]
    public int MixedCount { get; set; }

    [JsonPropertyName("is_show_mixed")]
    public bool IsShowMixed { get; set; }

    [JsonPropertyName("mblog_feed_back_menus_format")]
    public object[] MblogFeedBackMenusFormat { get; set; } = [];

    [JsonPropertyName("isSinglePayAudio")]
    public bool IsSinglePayAudio { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("text_raw")]
    public string TextRaw { get; set; } = string.Empty;

    [JsonPropertyName("region_name")]
    public string RegionName { get; set; } = string.Empty;

    [JsonPropertyName("page_info")]
    public PageInfoModel PageInfo { get; set; } = new();

    [JsonPropertyName("ok")]
    public int Ok { get; set; }
}

public class PicInfosValueModel
{
    [JsonPropertyName("thumbnail")]
    public LargestModel Thumbnail { get; set; } = new();

    [JsonPropertyName("bmiddle")]
    public LargestModel Bmiddle { get; set; } = new();

    [JsonPropertyName("large")]
    public LargestModel Large { get; set; } = new();

    [JsonPropertyName("original")]
    public LargestModel Original { get; set; } = new();

    [JsonPropertyName("largest")]
    public LargestModel Largest { get; set; } = new();

    [JsonPropertyName("mw2000")]
    public LargestModel Mw2000 { get; set; } = new();

    [JsonPropertyName("largecover")]
    public LargestModel Largecover { get; set; } = new();

    [JsonPropertyName("object_id")]
    public string ObjectId { get; set; } = string.Empty;

    [JsonPropertyName("pic_id")]
    public string PicId { get; set; } = string.Empty;

    [JsonPropertyName("photo_tag")]
    public int PhotoTag { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("pic_status")]
    public int PicStatus { get; set; }
}

public class LargestModel
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("cut_type")]
    public int CutType { get; set; }

    [JsonPropertyName("type")]
    public object? Type { get; set; }
}


public class PageInfoModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("page_id")]
    public string PageId { get; set; } = string.Empty;

    [JsonPropertyName("object_type")]
    public string ObjectType { get; set; } = string.Empty;

    [JsonPropertyName("object_id")]
    public string ObjectId { get; set; } = string.Empty;

    [JsonPropertyName("content1")]
    public string Content1 { get; set; } = string.Empty;

    [JsonPropertyName("content2")]
    public string Content2 { get; set; } = string.Empty;

    [JsonPropertyName("act_status")]
    public int ActStatus { get; set; }

    [JsonPropertyName("media_info")]
    public MediaInfoModel MediaInfo { get; set; } = new();

    [JsonPropertyName("page_pic")]
    public string PagePic { get; set; } = string.Empty;

    [JsonPropertyName("page_title")]
    public string PageTitle { get; set; } = string.Empty;

    [JsonPropertyName("page_url")]
    public string PageUrl { get; set; } = string.Empty;

    [JsonPropertyName("pic_info")]
    public PicInfoModel PicInfo { get; set; } = new();

    [JsonPropertyName("oid")]
    public string Oid { get; set; } = string.Empty;

    [JsonPropertyName("type_icon")]
    public string TypeIcon { get; set; } = string.Empty;

    [JsonPropertyName("author_id")]
    public string AuthorId { get; set; } = string.Empty;

    [JsonPropertyName("authorid")]
    public string Authorid { get; set; } = string.Empty;

    [JsonPropertyName("warn")]
    public string Warn { get; set; } = string.Empty;

    [JsonPropertyName("actionlog")]
    public ActionlogModel Actionlog { get; set; } = new();

    [JsonPropertyName("short_url")]
    public string ShortUrl { get; set; } = string.Empty;
}

public class PicInfoModel
{
    [JsonPropertyName("pic_big")]
    public PicBigModel PicBig { get; set; } = new();

    [JsonPropertyName("pic_small")]
    public PicSmallModel PicSmall { get; set; } = new();

    [JsonPropertyName("pic_middle")]
    public PicMiddleModel PicMiddle { get; set; } = new();
}

public class MediaInfoModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("stream_url")]
    public string StreamUrl { get; set; } = string.Empty;

    [JsonPropertyName("stream_url_hd")]
    public string StreamUrlHd { get; set; } = string.Empty;

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("h5_url")]
    public string H5Url { get; set; } = string.Empty;

    [JsonPropertyName("mp4_sd_url")]
    public string Mp4SdUrl { get; set; } = string.Empty;

    [JsonPropertyName("mp4_hd_url")]
    public string Mp4HdUrl { get; set; } = string.Empty;

    [JsonPropertyName("h265_mp4_hd")]
    public string H265Mp4Hd { get; set; } = string.Empty;

    [JsonPropertyName("h265_mp4_ld")]
    public string H265Mp4Ld { get; set; } = string.Empty;

    [JsonPropertyName("inch_4_mp4_hd")]
    public string Inch4Mp4Hd { get; set; } = string.Empty;

    [JsonPropertyName("inch_5_mp4_hd")]
    public string Inch5Mp4Hd { get; set; } = string.Empty;

    [JsonPropertyName("inch_5_5_mp4_hd")]
    public string Inch55Mp4Hd { get; set; } = string.Empty;

    [JsonPropertyName("mp4_720p_mp4")]
    public string Mp4720pMp4 { get; set; } = string.Empty;

    [JsonPropertyName("hevc_mp4_720p")]
    public string HevcMp4720p { get; set; } = string.Empty;

    [JsonPropertyName("prefetch_type")]
    public int PrefetchType { get; set; }

    [JsonPropertyName("prefetch_size")]
    public int PrefetchSize { get; set; }

    [JsonPropertyName("act_status")]
    public int ActStatus { get; set; }

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = string.Empty;

    [JsonPropertyName("media_id")]
    public string MediaId { get; set; } = string.Empty;

    [JsonPropertyName("origin_total_bitrate")]
    public int OriginTotalBitrate { get; set; }

    [JsonPropertyName("video_orientation")]
    public string VideoOrientation { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("forward_strategy")]
    public int ForwardStrategy { get; set; }

    [JsonPropertyName("search_scheme")]
    public string SearchScheme { get; set; } = string.Empty;

    [JsonPropertyName("is_short_video")]
    public int IsShortVideo { get; set; }

    [JsonPropertyName("vote_is_show")]
    public int VoteIsShow { get; set; }

    [JsonPropertyName("belong_collection")]
    public int BelongCollection { get; set; }

    [JsonPropertyName("titles_display_time")]
    public string TitlesDisplayTime { get; set; } = string.Empty;

    [JsonPropertyName("show_progress_bar")]
    public int ShowProgressBar { get; set; }

    [JsonPropertyName("show_mute_button")]
    public bool ShowMuteButton { get; set; }

    [JsonPropertyName("ext_info")]
    public ExtInfoModel ExtInfo { get; set; } = new();

    [JsonPropertyName("next_title")]
    public string NextTitle { get; set; } = string.Empty;

    [JsonPropertyName("kol_title")]
    public string KolTitle { get; set; } = string.Empty;

    [JsonPropertyName("play_completion_actions")]
    public PlayCompletionActionsItemModel[] PlayCompletionActions { get; set; } = [];

    [JsonPropertyName("video_publish_time")]
    public int VideoPublishTime { get; set; }

    [JsonPropertyName("play_loop_type")]
    public int PlayLoopType { get; set; }

    [JsonPropertyName("author_mid")]
    public string AuthorMid { get; set; } = string.Empty;

    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = string.Empty;

    [JsonPropertyName("extra_info")]
    public ExtraInfoModel ExtraInfo { get; set; } = new();

    [JsonPropertyName("video_download_strategy")]
    public VideoDownloadStrategyModel VideoDownloadStrategy { get; set; } = new();

    [JsonPropertyName("jump_to")]
    public int JumpTo { get; set; }

    [JsonPropertyName("big_pic_info")]
    public BigPicInfoModel BigPicInfo { get; set; } = new();

    [JsonPropertyName("online_users")]
    public string OnlineUsers { get; set; } = string.Empty;

    [JsonPropertyName("online_users_number")]
    public int OnlineUsersNumber { get; set; }

    [JsonPropertyName("ttl")]
    public int Ttl { get; set; }

    [JsonPropertyName("storage_type")]
    public string StorageType { get; set; } = string.Empty;

    [JsonPropertyName("is_keep_current_mblog")]
    public int IsKeepCurrentMblog { get; set; }

    [JsonPropertyName("has_recommend_video")]
    public int HasRecommendVideo { get; set; }

    [JsonPropertyName("author_info")]
    public AuthorInfoModel AuthorInfo { get; set; } = new();

    [JsonPropertyName("playback_list")]
    public PlaybackListItemModel[] PlaybackList { get; set; } = [];
}

public class PlaybackListItemModel
{
    [JsonPropertyName("meta")]
    public MetaModel Meta { get; set; } = new();

    [JsonPropertyName("play_info")]
    public PlayInfoModel PlayInfo { get; set; } = new();
}

public class PlayInfoModel
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("mime")]
    public string Mime { get; set; } = string.Empty;

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("bitrate")]
    public int Bitrate { get; set; }

    [JsonPropertyName("prefetch_range")]
    public string PrefetchRange { get; set; } = string.Empty;

    [JsonPropertyName("video_codecs")]
    public string VideoCodecs { get; set; } = string.Empty;

    [JsonPropertyName("fps")]
    public int Fps { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("sar")]
    public string Sar { get; set; } = string.Empty;

    [JsonPropertyName("audio_codecs")]
    public string AudioCodecs { get; set; } = string.Empty;

    [JsonPropertyName("audio_sample_rate")]
    public int AudioSampleRate { get; set; }

    [JsonPropertyName("quality_label")]
    public string QualityLabel { get; set; } = string.Empty;

    [JsonPropertyName("quality_class")]
    public string QualityClass { get; set; } = string.Empty;

    [JsonPropertyName("quality_desc")]
    public string QualityDesc { get; set; } = string.Empty;

    [JsonPropertyName("audio_channels")]
    public int AudioChannels { get; set; }

    [JsonPropertyName("audio_sample_fmt")]
    public string AudioSampleFmt { get; set; } = string.Empty;

    [JsonPropertyName("audio_bits_per_sample")]
    public int AudioBitsPerSample { get; set; }

    [JsonPropertyName("watermark")]
    public string Watermark { get; set; } = string.Empty;

    [JsonPropertyName("extension")]
    public ExtensionModel Extension { get; set; } = new();

    [JsonPropertyName("video_decoder")]
    public string VideoDecoder { get; set; } = string.Empty;

    [JsonPropertyName("prefetch_enabled")]
    public bool PrefetchEnabled { get; set; }

    [JsonPropertyName("tcp_receive_buffer")]
    public int TcpReceiveBuffer { get; set; }

    [JsonPropertyName("dolby_atmos")]
    public bool DolbyAtmos { get; set; }

    [JsonPropertyName("color_transfer")]
    public string ColorTransfer { get; set; } = string.Empty;

    [JsonPropertyName("stereo_video")]
    public int StereoVideo { get; set; }

    [JsonPropertyName("first_pkt_end_pos")]
    public int FirstPktEndPos { get; set; }
}

public class ExtensionModel
{
    [JsonPropertyName("transcode_info")]
    public TranscodeInfoModel TranscodeInfo { get; set; } = new();
}

public class TranscodeInfoModel
{
    [JsonPropertyName("pcdn_rule_id")]
    public int PcdnRuleId { get; set; }

    [JsonPropertyName("pcdn_jank")]
    public int PcdnJank { get; set; }

    [JsonPropertyName("origin_video_dr")]
    public string OriginVideoDr { get; set; } = string.Empty;

    [JsonPropertyName("ab_strategies")]
    public string AbStrategies { get; set; } = string.Empty;
}

public class MetaModel
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("quality_index")]
    public int QualityIndex { get; set; }

    [JsonPropertyName("quality_desc")]
    public string QualityDesc { get; set; } = string.Empty;

    [JsonPropertyName("quality_label")]
    public string QualityLabel { get; set; } = string.Empty;

    [JsonPropertyName("quality_class")]
    public string QualityClass { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("quality_group")]
    public int QualityGroup { get; set; }

    [JsonPropertyName("is_hidden")]
    public bool IsHidden { get; set; }
}

public class AuthorInfoModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("idstr")]
    public string Idstr { get; set; } = string.Empty;

    [JsonPropertyName("pc_new")]
    public int PcNew { get; set; }

    [JsonPropertyName("screen_name")]
    public string ScreenName { get; set; } = string.Empty;

    [JsonPropertyName("profile_image_url")]
    public string ProfileImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("profile_url")]
    public string ProfileUrl { get; set; } = string.Empty;

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("verified_type")]
    public int VerifiedType { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("weihao")]
    public string Weihao { get; set; } = string.Empty;

    [JsonPropertyName("verified_type_ext")]
    public int VerifiedTypeExt { get; set; }

    [JsonPropertyName("avatar_large")]
    public string AvatarLarge { get; set; } = string.Empty;

    [JsonPropertyName("avatar_hd")]
    public string AvatarHd { get; set; } = string.Empty;

    [JsonPropertyName("follow_me")]
    public bool FollowMe { get; set; }

    [JsonPropertyName("following")]
    public bool Following { get; set; }

    [JsonPropertyName("mbrank")]
    public int Mbrank { get; set; }

    [JsonPropertyName("mbtype")]
    public int Mbtype { get; set; }

    [JsonPropertyName("v_plus")]
    public int VPlus { get; set; }

    [JsonPropertyName("user_ability")]
    public int UserAbility { get; set; }

    [JsonPropertyName("planet_video")]
    public bool PlanetVideo { get; set; }

    [JsonPropertyName("verified_reason")]
    public string VerifiedReason { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("followers_count")]
    public int FollowersCount { get; set; }

    [JsonPropertyName("followers_count_str")]
    public string FollowersCountStr { get; set; } = string.Empty;

    [JsonPropertyName("friends_count")]
    public int FriendsCount { get; set; }

    [JsonPropertyName("statuses_count")]
    public int StatusesCount { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("svip")]
    public int Svip { get; set; }

    [JsonPropertyName("vvip")]
    public int Vvip { get; set; }

    [JsonPropertyName("cover_image_phone")]
    public string CoverImagePhone { get; set; } = string.Empty;
}

public class BigPicInfoModel
{
    [JsonPropertyName("pic_big")]
    public PicBigModel PicBig { get; set; } = new();

    [JsonPropertyName("pic_small")]
    public PicSmallModel PicSmall { get; set; } = new();

    [JsonPropertyName("pic_middle")]
    public PicMiddleModel PicMiddle { get; set; } = new();
}

public class PicMiddleModel
{
    [JsonPropertyName("height")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int Height { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int Width { get; set; }
}

public class PicSmallModel
{
    [JsonPropertyName("height")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int Height { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int Width { get; set; }
}

public class PicBigModel
{
    [JsonPropertyName("height")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int Height { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int Width { get; set; }
}

public class VideoDownloadStrategyModel
{
    [JsonPropertyName("abandon_download")]
    public int AbandonDownload { get; set; }
}

public class ExtraInfoModel
{
    [JsonPropertyName("sceneid")]
    public string Sceneid { get; set; } = string.Empty;
}

public class PlayCompletionActionsItemModel
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int Type { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    [JsonPropertyName("btn_code")]
    public int BtnCode { get; set; }

    [JsonPropertyName("show_position")]
    public int ShowPosition { get; set; }

    [JsonPropertyName("actionlog")]
    public ActionlogModel Actionlog { get; set; } = new();
}

public class ExtInfoModel
{
    [JsonPropertyName("video_orientation")]
    public string VideoOrientation { get; set; } = string.Empty;
}

public class ContentAuthListItemModel
{
    [JsonPropertyName("content_auth")]
    public int ContentAuth { get; set; }

    [JsonPropertyName("show_type")]
    public int ShowType { get; set; }

    [JsonPropertyName("rank")]
    public int Rank { get; set; }
}

public class TitleModel
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("base_color")]
    public int BaseColor { get; set; }

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; } = string.Empty;
}

public class TagStructItemModel
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = string.Empty;

    [JsonPropertyName("oid")]
    public string Oid { get; set; } = string.Empty;

    [JsonPropertyName("tag_type")]
    public int TagType { get; set; }

    [JsonPropertyName("tag_hidden")]
    public int TagHidden { get; set; }

    [JsonPropertyName("tag_scheme")]
    public string TagScheme { get; set; } = string.Empty;

    [JsonPropertyName("url_type_pic")]
    public string UrlTypePic { get; set; } = string.Empty;

    [JsonPropertyName("actionlog")]
    public ActionlogModel Actionlog { get; set; } = new();

    [JsonPropertyName("bd_object_type")]
    public string BdObjectType { get; set; } = string.Empty;

    [JsonPropertyName("w_h_ratio")]
    [JsonConverter(typeof(FlexibleDoubleConverter))]
    public double WHRatio { get; set; }
}

public class UrlStructItemModel
{
    [JsonPropertyName("url_title")]
    public string UrlTitle { get; set; } = string.Empty;

    [JsonPropertyName("url_type_pic")]
    public string UrlTypePic { get; set; } = string.Empty;

    [JsonPropertyName("ori_url")]
    public string OriUrl { get; set; } = string.Empty;

    [JsonPropertyName("page_id")]
    public string PageId { get; set; } = string.Empty;

    [JsonPropertyName("short_url")]
    public string ShortUrl { get; set; } = string.Empty;

    [JsonPropertyName("long_url")]
    public string LongUrl { get; set; } = string.Empty;

    [JsonPropertyName("url_type")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int UrlType { get; set; }

    [JsonPropertyName("result")]
    public bool Result { get; set; }

    [JsonPropertyName("actionlog")]
    public ActionlogModel Actionlog { get; set; } = new();

    [JsonPropertyName("storage_type")]
    public string StorageType { get; set; } = string.Empty;

    [JsonPropertyName("hide")]
    public int Hide { get; set; }

    [JsonPropertyName("object_type")]
    public string ObjectType { get; set; } = string.Empty;

    [JsonPropertyName("ttl")]
    public int Ttl { get; set; }

    [JsonPropertyName("h5_target_url")]
    public string H5TargetUrl { get; set; } = string.Empty;

    [JsonPropertyName("need_save_obj")]
    public int NeedSaveObj { get; set; }
}

public class TopicStructItemModel
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("topic_url")]
    public string TopicUrl { get; set; } = string.Empty;

    [JsonPropertyName("topic_title")]
    public string TopicTitle { get; set; } = string.Empty;

    [JsonPropertyName("actionlog")]
    public ActionlogModel Actionlog { get; set; } = new();
}

public class ActionlogModel
{
    [JsonPropertyName("act_type")]
    public int ActType { get; set; }

    [JsonPropertyName("act_code")]
    public int ActCode { get; set; }

    [JsonPropertyName("oid")]
    public string Oid { get; set; } = string.Empty;

    [JsonPropertyName("uuid")]
    public long Uuid { get; set; }

    [JsonPropertyName("cardid")]
    public string Cardid { get; set; } = string.Empty;

    [JsonPropertyName("lcardid")]
    public string Lcardid { get; set; } = string.Empty;

    [JsonPropertyName("uicode")]
    public string Uicode { get; set; } = string.Empty;

    [JsonPropertyName("luicode")]
    public string Luicode { get; set; } = string.Empty;

    [JsonPropertyName("fid")]
    public string Fid { get; set; } = string.Empty;

    [JsonPropertyName("lfid")]
    public string Lfid { get; set; } = string.Empty;

    [JsonPropertyName("ext")]
    public string Ext { get; set; } = string.Empty;
}

public class CommentManageInfoModel
{
    [JsonPropertyName("comment_permission_type")]
    public int CommentPermissionType { get; set; }

    [JsonPropertyName("approval_comment_type")]
    public int ApprovalCommentType { get; set; }

    [JsonPropertyName("comment_sort_type")]
    public int CommentSortType { get; set; }
}

public class NumberDisplayStrategyModel
{
    [JsonPropertyName("apply_scenario_flag")]
    public int ApplyScenarioFlag { get; set; }

    [JsonPropertyName("display_text_min_number")]
    public int DisplayTextMinNumber { get; set; }

    [JsonPropertyName("display_text")]
    public string DisplayText { get; set; } = string.Empty;
}

public class AnnotationsItemModel
{
    [JsonPropertyName("photo_sub_type")]
    public string PhotoSubType { get; set; } = string.Empty;
}

public class UserModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("idstr")]
    public string Idstr { get; set; } = string.Empty;

    [JsonPropertyName("pc_new")]
    public int PcNew { get; set; }

    [JsonPropertyName("screen_name")]
    public string ScreenName { get; set; } = string.Empty;

    [JsonPropertyName("profile_image_url")]
    public string ProfileImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("profile_url")]
    public string ProfileUrl { get; set; } = string.Empty;

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("verified_type")]
    public int VerifiedType { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("weihao")]
    public string Weihao { get; set; } = string.Empty;

    [JsonPropertyName("verified_type_ext")]
    public int VerifiedTypeExt { get; set; }

    [JsonPropertyName("avatar_large")]
    public string AvatarLarge { get; set; } = string.Empty;

    [JsonPropertyName("avatar_hd")]
    public string AvatarHd { get; set; } = string.Empty;

    [JsonPropertyName("follow_me")]
    public bool FollowMe { get; set; }

    [JsonPropertyName("following")]
    public bool Following { get; set; }

    [JsonPropertyName("mbrank")]
    public int Mbrank { get; set; }

    [JsonPropertyName("mbtype")]
    public int Mbtype { get; set; }

    [JsonPropertyName("v_plus")]
    public int VPlus { get; set; }

    [JsonPropertyName("user_ability")]
    public int UserAbility { get; set; }

    [JsonPropertyName("planet_video")]
    public bool PlanetVideo { get; set; }

    [JsonPropertyName("icon_list")]
    public IconListItemModel[] IconList { get; set; } = [];
}

public class IconListItemModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public DataModel Data { get; set; } = new();
}

public class DataModel
{
    [JsonPropertyName("mbrank")]
    public int Mbrank { get; set; }

    [JsonPropertyName("mbtype")]
    public int Mbtype { get; set; }

    [JsonPropertyName("svip")]
    public int Svip { get; set; }

    [JsonPropertyName("vvip")]
    public int Vvip { get; set; }
}

public class VisibleModel
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("list_id")]
    public int ListId { get; set; }
}

