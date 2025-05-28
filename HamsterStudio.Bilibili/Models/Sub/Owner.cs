using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
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
        public string Face { get; set; }
    }
}