using System.Text.Json.Serialization;

namespace HamsterStudio.BeautyLegs.RedBook.Sub
{
    public class StreamInfoModel
    {
        [JsonPropertyName("masterUrl")]
        public string MasterUrl { get; set; } = string.Empty;

        [JsonPropertyName("backupUrls")]
        public List<string> BackupUrls { get; set; } = [];
    }
}
