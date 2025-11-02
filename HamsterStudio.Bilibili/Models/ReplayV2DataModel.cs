using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models;

public class ReplayV2DataModel
{
    [JsonPropertyName("page")]
    public PageModel Page { get; set; } = new();

    [JsonPropertyName("config")]
    public ConfigModel Config { get; set; } = new();

    [JsonPropertyName("replies")]
    public RepliesItemModel[] Replies { get; set; } = [];

    [JsonPropertyName("upper")]
    public UpperModel Upper { get; set; } = new();

    [JsonPropertyName("top")]
    public object? Top { get; set; }

    [JsonPropertyName("vote")]
    public int Vote { get; set; }

    [JsonPropertyName("blacklist")]
    public int Blacklist { get; set; }

    [JsonPropertyName("assist")]
    public int Assist { get; set; }

    [JsonPropertyName("mode")]
    public int Mode { get; set; }

    [JsonPropertyName("control")]
    public ControlModel Control { get; set; } = new();

    [JsonPropertyName("folder")]
    public FolderModel Folder { get; set; } = new();
}

public class ControlModel
{
    [JsonPropertyName("input_disable")]
    public bool InputDisable { get; set; }

    [JsonPropertyName("root_input_text")]
    public string RootInputText { get; set; } = string.Empty;

    [JsonPropertyName("child_input_text")]
    public string ChildInputText { get; set; } = string.Empty;

    [JsonPropertyName("giveup_input_text")]
    public string GiveupInputText { get; set; } = string.Empty;

    [JsonPropertyName("screenshot_icon_state")]
    public int ScreenshotIconState { get; set; }

    [JsonPropertyName("upload_picture_icon_state")]
    public int UploadPictureIconState { get; set; }

    [JsonPropertyName("answer_guide_text")]
    public string AnswerGuideText { get; set; } = string.Empty;

    [JsonPropertyName("answer_guide_icon_url")]
    public string AnswerGuideIconUrl { get; set; } = string.Empty;

    [JsonPropertyName("answer_guide_ios_url")]
    public string AnswerGuideIosUrl { get; set; } = string.Empty;

    [JsonPropertyName("answer_guide_android_url")]
    public string AnswerGuideAndroidUrl { get; set; } = string.Empty;

    [JsonPropertyName("bg_text")]
    public string BgText { get; set; } = string.Empty;

    [JsonPropertyName("empty_page")]
    public object? EmptyPage { get; set; }

    [JsonPropertyName("show_type")]
    public int ShowType { get; set; }

    [JsonPropertyName("show_text")]
    public string ShowText { get; set; } = string.Empty;

    [JsonPropertyName("web_selection")]
    public bool WebSelection { get; set; }

    [JsonPropertyName("disable_jump_emote")]
    public bool DisableJumpEmote { get; set; }

    [JsonPropertyName("enable_charged")]
    public bool EnableCharged { get; set; }

    [JsonPropertyName("enable_cm_biz_helper")]
    public bool EnableCmBizHelper { get; set; }

    [JsonPropertyName("preload_resources")]
    public object? PreloadResources { get; set; }
}

public class UpperModel
{
    [JsonPropertyName("mid")]
    public int Mid { get; set; }

    [JsonPropertyName("top")]
    public object? Top { get; set; }

    [JsonPropertyName("vote")]
    public object? Vote { get; set; }
}

public class RepliesItemModel
{
    [JsonPropertyName("rpid")]
    public long Rpid { get; set; }

    [JsonPropertyName("oid")]
    public long Oid { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("mid")]
    public long Mid { get; set; }

    [JsonPropertyName("root")]
    public long Root { get; set; }

    [JsonPropertyName("parent")]
    public long Parent { get; set; }

    //[JsonPropertyName("dialog")]
    //public int Dialog { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("rcount")]
    public int Rcount { get; set; }

    [JsonPropertyName("state")]
    public int State { get; set; }

    [JsonPropertyName("fansgrade")]
    public int Fansgrade { get; set; }

    [JsonPropertyName("attr")]
    public int Attr { get; set; }

    [JsonPropertyName("ctime")]
    public long Ctime { get; set; }

    [JsonPropertyName("mid_str")]
    public string MidStr { get; set; } = string.Empty;

    [JsonPropertyName("oid_str")]
    public string OidStr { get; set; } = string.Empty;

    [JsonPropertyName("rpid_str")]
    public string RpidStr { get; set; } = string.Empty;

    [JsonPropertyName("root_str")]
    public string RootStr { get; set; } = string.Empty;

    [JsonPropertyName("parent_str")]
    public string ParentStr { get; set; } = string.Empty;

    [JsonPropertyName("dialog_str")]
    public string DialogStr { get; set; } = string.Empty;

    [JsonPropertyName("like")]
    public int Like { get; set; }

    [JsonPropertyName("action")]
    public int Action { get; set; }

    [JsonPropertyName("member")]
    public MemberModel Member { get; set; } = new();

    [JsonPropertyName("content")]
    public ContentModel Content { get; set; } = new();

    [JsonPropertyName("replies")]
    public RepliesItemModel[] Replies { get; set; } = [];

    [JsonPropertyName("assist")]
    public int Assist { get; set; }

    [JsonPropertyName("up_action")]
    public UpActionModel UpAction { get; set; } = new();

    [JsonPropertyName("invisible")]
    public bool Invisible { get; set; }

    [JsonPropertyName("reply_control")]
    public ReplyControlModel ReplyControl { get; set; } = new();

    [JsonPropertyName("folder")]
    public FolderModel Folder { get; set; } = new();

    [JsonPropertyName("dynamic_id_str")]
    public string DynamicIdStr { get; set; } = string.Empty;

    [JsonPropertyName("note_cvid_str")]
    public string NoteCvidStr { get; set; } = string.Empty;

    [JsonPropertyName("track_info")]
    public string TrackInfo { get; set; } = string.Empty;
}

public class FolderModel
{
    [JsonPropertyName("has_folded")]
    public bool HasFolded { get; set; }

    [JsonPropertyName("is_folded")]
    public bool IsFolded { get; set; }

    [JsonPropertyName("rule")]
    public string Rule { get; set; } = string.Empty;
}

public class ReplyControlModel
{
    [JsonPropertyName("max_line")]
    public int MaxLine { get; set; }

    [JsonPropertyName("time_desc")]
    public string TimeDesc { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("translation_switch")]
    public int TranslationSwitch { get; set; }

    [JsonPropertyName("support_share")]
    public bool SupportShare { get; set; }
}

public class UpActionModel
{
    [JsonPropertyName("like")]
    public bool Like { get; set; }

    [JsonPropertyName("reply")]
    public bool Reply { get; set; }
}

public class ContentModel
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("members")]
    public object[] Members { get; set; } = [];

    [JsonPropertyName("max_line")]
    public int MaxLine { get; set; }

    [JsonPropertyName("pictures")]
    public ReplayPictureModel[] Pictures { get; set; } = [];
}

public class ReplayPictureModel
{
    [JsonPropertyName("img_src")]
    public string ImageSrc { get; set; } = string.Empty;

    [JsonPropertyName("img_width")]
    public long Width { get; set; }

    [JsonPropertyName("img_height")]
    public long Height { get; set; }

    [JsonPropertyName("img_size")]
    public decimal Size { get; set; }
}

public class MemberModel
{
    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;

    [JsonPropertyName("uname")]
    public string Uname { get; set; } = string.Empty;

    [JsonPropertyName("sex")]
    public string Sex { get; set; } = string.Empty;

    [JsonPropertyName("sign")]
    public string Sign { get; set; } = string.Empty;

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = string.Empty;

    [JsonPropertyName("rank")]
    public string Rank { get; set; } = string.Empty;

    [JsonPropertyName("face_nft_new")]
    public int FaceNftNew { get; set; }

    [JsonPropertyName("is_senior_member")]
    public int IsSeniorMember { get; set; }

    [JsonPropertyName("senior")]
    public SeniorModel Senior { get; set; } = new();

    [JsonPropertyName("level_info")]
    public LevelInfoModel LevelInfo { get; set; } = new();

    //[JsonPropertyName("pendant")]
    //public PendantModel Pendant { get; set; } = new();

    [JsonPropertyName("nameplate")]
    public NameplateModel Nameplate { get; set; } = new();

    [JsonPropertyName("official_verify")]
    public OfficialVerifyModel OfficialVerify { get; set; } = new();

    [JsonPropertyName("vip")]
    public VipModel Vip { get; set; } = new();

    [JsonPropertyName("fans_detail")]
    public object? FansDetail { get; set; }

    //[JsonPropertyName("user_sailing")]
    //public UserSailingModel UserSailing { get; set; } = new();

    //[JsonPropertyName("user_sailing_v2")]
    //public UserSailingV2Model UserSailingV2 { get; set; } = new();

    [JsonPropertyName("is_contractor")]
    public bool IsContractor { get; set; }

    [JsonPropertyName("contract_desc")]
    public string ContractDesc { get; set; } = string.Empty;

    [JsonPropertyName("nft_interaction")]
    public object? NftInteraction { get; set; }

    [JsonPropertyName("avatar_item")]
    public AvatarItemModel AvatarItem { get; set; } = new();
}

public class AvatarItemModel
{
    //[JsonPropertyName("container_size")]
    //public ContainerSizeModel ContainerSize { get; set; } = new();

    //[JsonPropertyName("fallback_layers")]
    //public FallbackLayersModel FallbackLayers { get; set; } = new();

    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;
}

public class FallbackLayersModel
{
    [JsonPropertyName("layers")]
    public LayersItemModel[] Layers { get; set; } = [];

    [JsonPropertyName("is_critical_group")]
    public bool IsCriticalGroup { get; set; }
}

public class LayersItemModel
{
    [JsonPropertyName("visible")]
    public bool Visible { get; set; }

    [JsonPropertyName("general_spec")]
    public GeneralSpecModel GeneralSpec { get; set; } = new();

    [JsonPropertyName("layer_config")]
    public LayerConfigModel LayerConfig { get; set; } = new();

    [JsonPropertyName("resource")]
    public ResourceModel Resource { get; set; } = new();
}

public class ResourceModel
{
    [JsonPropertyName("res_type")]
    public int ResType { get; set; }

    [JsonPropertyName("res_image")]
    public ResImageModel ResImage { get; set; } = new();
}

public class ResImageModel
{
    [JsonPropertyName("image_src")]
    public ImageSrcModel ImageSrc { get; set; } = new();
}

public class ImageSrcModel
{
    [JsonPropertyName("src_type")]
    public int SrcType { get; set; }

    [JsonPropertyName("placeholder")]
    public int Placeholder { get; set; }

    [JsonPropertyName("remote")]
    public RemoteModel Remote { get; set; } = new();
}

public class RemoteModel
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("bfs_style")]
    public string BfsStyle { get; set; } = string.Empty;
}

public class LayerConfigModel
{
    [JsonPropertyName("tags")]
    public TagsModel Tags { get; set; } = new();

    [JsonPropertyName("is_critical")]
    public bool IsCritical { get; set; }
}

public class TagsModel
{
    [JsonPropertyName("AVATAR_LAYER")]
    public AVATARLAYERModel AVATARLAYER { get; set; } = new();

    [JsonPropertyName("GENERAL_CFG")]
    public GENERALCFGModel GENERALCFG { get; set; } = new();
}

public class GENERALCFGModel
{
    [JsonPropertyName("config_type")]
    public int ConfigType { get; set; }

    [JsonPropertyName("general_config")]
    public GeneralConfigModel GeneralConfig { get; set; } = new();
}

public class GeneralConfigModel
{
    [JsonPropertyName("web_css_style")]
    public WebCssStyleModel WebCssStyle { get; set; } = new();
}

public class WebCssStyleModel
{
    [JsonPropertyName("borderRadius")]
    public string BorderRadius { get; set; } = string.Empty;
}

public class AVATARLAYERModel
{

}

public class GeneralSpecModel
{
    [JsonPropertyName("pos_spec")]
    public PosSpecModel PosSpec { get; set; } = new();

    [JsonPropertyName("size_spec")]
    public SizeSpecModel SizeSpec { get; set; } = new();

    [JsonPropertyName("render_spec")]
    public RenderSpecModel RenderSpec { get; set; } = new();
}

public class RenderSpecModel
{
    [JsonPropertyName("opacity")]
    public int Opacity { get; set; }
}

public class SizeSpecModel
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class PosSpecModel
{
    [JsonPropertyName("coordinate_pos")]
    public int CoordinatePos { get; set; }

    [JsonPropertyName("axis_x")]
    public double AxisX { get; set; }

    [JsonPropertyName("axis_y")]
    public double AxisY { get; set; }
}

public class UserSailingV2Model
{
    [JsonPropertyName("card_bg")]
    public CardBgModel CardBg { get; set; } = new();
}

public class CardBgModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("jump_url")]
    public string JumpUrl { get; set; } = string.Empty;

    [JsonPropertyName("fan")]
    public FanModel Fan { get; set; } = new();

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class UserSailingModel
{
    [JsonPropertyName("pendant")]
    public object? Pendant { get; set; }

    [JsonPropertyName("cardbg")]
    public CardbgModel Cardbg { get; set; } = new();

    [JsonPropertyName("cardbg_with_focus")]
    public object? CardbgWithFocus { get; set; }
}

public class CardbgModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("jump_url")]
    public string JumpUrl { get; set; } = string.Empty;

    [JsonPropertyName("fan")]
    public FanModel Fan { get; set; } = new();

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("image_group")]
    public object? ImageGroup { get; set; }
}

public class NameplateModel
{
    [JsonPropertyName("nid")]
    public int Nid { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("image_small")]
    public string ImageSmall { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("condition")]
    public string Condition { get; set; } = string.Empty;
}

public class LevelInfoModel
{
    [JsonPropertyName("current_level")]
    public int CurrentLevel { get; set; }

    [JsonPropertyName("current_min")]
    public int CurrentMin { get; set; }

    [JsonPropertyName("current_exp")]
    public int CurrentExp { get; set; }

    [JsonPropertyName("next_exp")]
    public int NextExp { get; set; }
}

public class SeniorModel
{
    [JsonPropertyName("status")]
    public int Status { get; set; }
}

public class ConfigModel
{
    [JsonPropertyName("showtopic")]
    public int Showtopic { get; set; }

    [JsonPropertyName("show_up_flag")]
    public bool ShowUpFlag { get; set; }

    [JsonPropertyName("read_only")]
    public bool ReadOnly { get; set; }
}

public class PageModel
{
    [JsonPropertyName("num")]
    public int Num { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("acount")]
    public int Acount { get; set; }
}
