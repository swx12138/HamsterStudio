using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Live;

public class DashUrl
{
    [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
    [JsonPropertyName("length")] public int Length { get; init; } = 0;
    [JsonPropertyName("order")] public int Order { get; init; } = 0;
    [JsonPropertyName("stream_type")] public int StreamType { get; init; } = 0;
    [JsonPropertyName("p2p_type")] public int P2PType { get; init; } = 0;

}

public class QualityDescription
{

    [JsonPropertyName("qn")]
    public int CurrentQualityNumber { get; init; }

    [JsonPropertyName("desc")]
    public string Description { get; init; } = string.Empty;
}

public class PlayUrlData
{
    [JsonPropertyName("current_quality")] public int CurrentQuality { get; init; }

    [JsonPropertyName("current_qn")] public int CurrentQualityNumber { get; init; }

    [JsonPropertyName("accept_quality")] public string[] AcceptQuality { get; init; } = [];

    [JsonPropertyName("quality_description")] public QualityDescription[] QualityDescription { get; init; } = [];

    [JsonPropertyName("durl")] public DashUrl[] DashUrls { get; init; } = [];

}
