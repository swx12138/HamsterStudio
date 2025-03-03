using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili.SubStruct
{
    public struct Owner
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("mid")]
        public UInt128 Mid { get; set; }

        /// <summary>
        /// 焖焖碳-
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("face")]
        public string Face { get; set; }
    }
}