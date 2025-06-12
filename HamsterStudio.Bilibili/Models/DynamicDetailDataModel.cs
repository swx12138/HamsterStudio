using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HamsterStudio.Bilibili.Models;

public class DynamicDetailDataModel
{
    [JsonPropertyName("item")]
    public ItemModel Item { get; set; } = new();
}

public class ItemModel
{
    [JsonPropertyName("id_str")]
    public string IdStr { get; set; } = string.Empty;

    [JsonPropertyName("modules")]
    public ModulesModel Modules { get; set; } = new();

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("visible")]
    public bool Visible { get; set; }
}

public class ModulesModel
{
    [JsonPropertyName("module_author")]
    public ModuleAuthorModel ModuleAuthor { get; set; } = new();

    [JsonPropertyName("module_dynamic")]
    public ModuleDynamicModel ModuleDynamic { get; set; } = new();

    [JsonPropertyName("module_stat")]
    public ModuleStatModel ModuleStat { get; set; } = new();
}

public class ModuleStatModel
{
    [JsonPropertyName("comment")]
    public CommentModel Comment { get; set; } = new();

    [JsonPropertyName("forward")]
    public ForwardModel Forward { get; set; } = new();

    [JsonPropertyName("like")]
    public LikeModel Like { get; set; } = new();
}

public class LikeModel
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("forbidden")]
    public bool Forbidden { get; set; }

    [JsonPropertyName("status")]
    public bool Status { get; set; }
}

public class ForwardModel
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("forbidden")]
    public bool Forbidden { get; set; }
}

public class CommentModel
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("forbidden")]
    public bool Forbidden { get; set; }
}

public class ModuleDynamicModel
{
    [JsonPropertyName("additional")]
    public object? Additional { get; set; }

    [JsonPropertyName("desc")]
    public DescModel Desc { get; set; } = new();

    [JsonPropertyName("major")]
    public MajorModel Major { get; set; } = new();

    [JsonPropertyName("topic")]
    public object? Topic { get; set; }
}

public class MajorModel
{
    [JsonPropertyName("draw")]
    public DrawModel Draw { get; set; } = new();

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class DrawModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("items")]
    public ItemsItemModel[] Items { get; set; } = [];
}

public class ItemsItemModel
{
    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("size")]
    public double Size { get; set; }

    [JsonPropertyName("src")]
    public string Src { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public object[] Tags { get; set; } = [];

    [JsonPropertyName("width")]
    public int Width { get; set; }
}

public class DescModel
{
    [JsonPropertyName("rich_text_nodes")]
    public RichTextNodesItemModel[] RichTextNodes { get; set; } = [];

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class RichTextNodesItemModel
{
    [JsonPropertyName("orig_text")]
    public string OrigText { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class ModuleAuthorModel
{
    [JsonPropertyName("avatar")]
    public AvatarModel Avatar { get; set; } = new();

    [JsonPropertyName("decorate")]
    public DecorateModel Decorate { get; set; } = new();

    [JsonPropertyName("face")]
    public string Face { get; set; } = string.Empty;

    [JsonPropertyName("face_nft")]
    public bool FaceNft { get; set; }

    [JsonPropertyName("following")]
    public object? Following { get; set; }

    [JsonPropertyName("jump_url")]
    public string JumpUrl { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("mid")]
    public int Mid { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("official_verify")]
    public OfficialVerifyModel OfficialVerify { get; set; } = new();

    [JsonPropertyName("pendant")]
    public PendantModel Pendant { get; set; } = new();

    [JsonPropertyName("pub_action")]
    public string PubAction { get; set; } = string.Empty;

    [JsonPropertyName("pub_location_text")]
    public string PubLocationText { get; set; } = string.Empty;

    [JsonPropertyName("pub_time")]
    public string PubTime { get; set; } = string.Empty;

    [JsonPropertyName("pub_ts")]
    public int PubTs { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("vip")]
    public VipModel Vip { get; set; } = new();
}

public class VipModel
{
    [JsonPropertyName("avatar_subscript")]
    public int AvatarSubscript { get; set; }

    [JsonPropertyName("avatar_subscript_url")]
    public string AvatarSubscriptUrl { get; set; } = string.Empty;

    [JsonPropertyName("due_date")]
    public long DueDate { get; set; }

    [JsonPropertyName("label")]
    public LabelModel Label { get; set; } = new();

    [JsonPropertyName("nickname_color")]
    public string NicknameColor { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("theme_type")]
    public int ThemeType { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }
}

public class LabelModel
{
    [JsonPropertyName("bg_color")]
    public string BgColor { get; set; } = string.Empty;

    [JsonPropertyName("bg_style")]
    public int BgStyle { get; set; }

    [JsonPropertyName("border_color")]
    public string BorderColor { get; set; } = string.Empty;

    [JsonPropertyName("img_label_uri_hans")]
    public string ImgLabelUriHans { get; set; } = string.Empty;

    [JsonPropertyName("img_label_uri_hans_static")]
    public string ImgLabelUriHansStatic { get; set; } = string.Empty;

    [JsonPropertyName("img_label_uri_hant")]
    public string ImgLabelUriHant { get; set; } = string.Empty;

    [JsonPropertyName("img_label_uri_hant_static")]
    public string ImgLabelUriHantStatic { get; set; } = string.Empty;

    [JsonPropertyName("label_theme")]
    public string LabelTheme { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("text_color")]
    public string TextColor { get; set; } = string.Empty;

    [JsonPropertyName("use_img_label")]
    public bool UseImgLabel { get; set; }
}

public class PendantModel
{
    [JsonPropertyName("expire")]
    public int Expire { get; set; }

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("image_enhance")]
    public string ImageEnhance { get; set; } = string.Empty;

    [JsonPropertyName("image_enhance_frame")]
    public string ImageEnhanceFrame { get; set; } = string.Empty;

    [JsonPropertyName("n_pid")]
    public int NPid { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("pid")]
    public int Pid { get; set; }
}

public class OfficialVerifyModel
{
    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; set; }
}

public class DecorateModel
{
    [JsonPropertyName("card_url")]
    public string CardUrl { get; set; } = string.Empty;

    [JsonPropertyName("fan")]
    public FanModel Fan { get; set; } = new();

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("jump_url")]
    public string JumpUrl { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; set; }
}

public class FanModel
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("color_format")]
    public object? ColorFormat { get; set; }

    [JsonPropertyName("is_fan")]
    public bool IsFan { get; set; }

    [JsonPropertyName("num_prefix")]
    public string NumPrefix { get; set; } = string.Empty;

    [JsonPropertyName("num_str")]
    public string NumStr { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public int Number { get; set; }
}

public class AvatarModel
{
    [JsonPropertyName("container_size")]
    public ContainerSizeModel ContainerSize { get; set; } = new();

    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;
}

public class ContainerSizeModel
{
    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; }
}

