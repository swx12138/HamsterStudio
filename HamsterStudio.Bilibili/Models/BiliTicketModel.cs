using System.Text.Json.Serialization;

namespace HamsterStudio.Bilibili.Models;

public class BiliTicketModel
{

    [JsonPropertyName("created_at")]
    public ulong CreateAt { get; set; }

    [JsonPropertyName("ttl")]
    public int TTL { get; set; }

    [JsonPropertyName("nav")]
    public NavModel Nav { get; set; } = new();
}
public class NavModel
{
    [JsonPropertyName("img")]
    public string Img { get; set; } = string.Empty;

    [JsonPropertyName("sub")]
    public string Sub { get; set; } = string.Empty;

}
