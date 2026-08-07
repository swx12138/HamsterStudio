using HamsterStudio.Bilibili.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models;

public class ReplayV2DataModel
{
    [JsonPropertyName("page")]
    public PageModel Page { get; set; } = new();

    [JsonPropertyName("replies")]
    public RepliesItemModel[] Replies { get; set; } = [];
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

public class SizeSpecModel
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
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

public class SeniorModel
{
    [JsonPropertyName("status")]
    public int Status { get; set; }
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
