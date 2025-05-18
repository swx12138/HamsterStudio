using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HamsterStudio.BraveShine.Models;

public class ServerRespenseModel
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<string> Files { get; set; } = [];

}
