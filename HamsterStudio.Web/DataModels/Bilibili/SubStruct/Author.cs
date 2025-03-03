using System.Text.Json.Serialization;

namespace HamsterStudio.Web.DataModels.Bilibili.SubStruct
{
    public struct Author
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("mid")]
        public long Mid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("sex")]
        public string Sex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("face")]
        public string Face { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("sign")]
        public string Sign { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("rank")]
        public long Rank { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("birthday")]
        public long Birthday { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_fake_account")]
        public long IsFakeAccount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_deleted")]
        public long IsDeleted { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("in_reg_audit")]
        public long InRegAudit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_senior_member")]
        public long IsSeniorMember { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("name_render")]
        public string NameRender { get; set; }
    }
}
