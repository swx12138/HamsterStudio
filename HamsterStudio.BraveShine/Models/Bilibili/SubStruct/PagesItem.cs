using HamsterStudio.HandyUtil.PropertyEditors;
using HandyControl.Controls;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HamsterStudio.BraveShine.Models.Bilibili.SubStruct
{
    public struct PagesItem
    {
        [JsonPropertyName("cid")]
        public long Cid { get; set; }

        [JsonPropertyName("page")]
        public long Page { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        /// <summary>
        /// 谢谢你帮我捡鞋子(◜𖥦◝ ）
        /// </summary>
        [JsonPropertyName("part")]
        public string Part { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("vid")]
        public string Vid { get; set; }

        [JsonPropertyName("weblink")]
        public string Weblink { get; set; }

        [JsonPropertyName("dimension")]
        public Dimension Dimension { get; set; }

        [JsonPropertyName("first_frame")]
        [Editor(typeof(ImageViewOnlyEditor), typeof(PropertyEditorBase))]
        public string FirstFrame { get; set; }

    }
}