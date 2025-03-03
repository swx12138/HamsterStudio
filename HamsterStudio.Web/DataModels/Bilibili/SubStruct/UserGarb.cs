using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili.SubStruct
{
    public struct UserGarb
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("url_image_ani_cut")]
        public string UrlImageAniCut { get; set; }
    }
}