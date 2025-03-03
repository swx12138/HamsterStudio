using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili.SubStruct
{
    public struct Dolby
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("type")]
        public long Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("audio")]
        public string Audio { get; set; }
    }
}