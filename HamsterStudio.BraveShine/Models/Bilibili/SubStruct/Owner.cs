using HamsterStudio.BraveShine.PropertyEditors;
using HandyControl.Controls;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HamsterStudio.BraveShine.Models.Bilibili.SubStruct
{
    public struct Owner
    {

        [JsonPropertyName("mid")]
        [Editable(false)]
        public UInt128 Mid { get; set; }

        /// <summary>
        /// 焖焖碳-
        /// </summary>
        [JsonPropertyName("name")]
        [Editable(false), Category("User")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("face")]
        [Editable(false), Category("User")]
        [Editor(typeof(ImageViewOnlyEditor), typeof(PropertyEditorBase))]
        public string Face { get; set; }
    }
}