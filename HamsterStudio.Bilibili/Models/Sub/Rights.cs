using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public struct Rights
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("bp")]
        public long Bp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("elec")]
        public long Elec { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("download")]
        public long Download { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("movie")]
        public long Movie { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("pay")]
        public long Pay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("hd5")]
        public long Hd5 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("no_reprint")]
        public long NoReprint { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("autoplay")]
        public long Autoplay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ugc_pay")]
        public long UgcPay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_cooperation")]
        public long IsCooperation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ugc_pay_preview")]
        public long UgcPayPreview { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("no_background")]
        public long NoBackground { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("clean_mode")]
        public long CleanMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_stein_gate")]
        public long IsSteinGate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("is_360")]
        public long Is360 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("no_share")]
        public long NoShare { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("arc_pay")]
        public long ArcPay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("free_watch")]
        public long FreeWatch { get; set; }
    }
}