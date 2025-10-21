using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models.Live;

public class RoomInfoOld
{
    [JsonPropertyName("roomStatus")] public int RoomStatus { get; init; }

    [JsonPropertyName("roundStatus")] public int RoundStatus { get; init; }

    [JsonPropertyName("liveStatus")] public int LiveStatus { get; init; }

    [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;

    [JsonPropertyName("title")] public string Title { get; init; } = string.Empty;

    [JsonPropertyName("cover")] public string Cover { get; init; } = string.Empty;

    [JsonPropertyName("online")] public int Online { get; init; }

    [JsonPropertyName("roomid")] public int RoomId { get; init; }

}
