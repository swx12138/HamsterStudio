using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Sub
{
    public struct DashAvInfo
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("base_url")]
        public string BaseUrl2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("backupUrl")]
        public List<string> BackupUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("backup_url")]
        public List<string> BackupUrl2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("bandwidth")]
        public long Bandwidth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("mime_type")]
        public string MimeType2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("codecs")]
        public string Codecs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("width")]
        public long Width { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("height")]
        public long Height { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("frameRate")]
        public string FrameRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("frame_rate")]
        public string FrameRate2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("sar")]
        public string Sar { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("startWithSap")]
        public long StartWithSap { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("start_with_sap")]
        public long StartWithSap2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SegmentBase SegmentBase { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("segment_base")]
        public SegmentBase SegmentBase2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("codecid")]
        public long Codecid { get; set; }
    }
}