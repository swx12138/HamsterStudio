using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HamsterStudio.Web.DataModels.ReadBook;

public class PostBodyModel
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("download")]
    public bool Download { get; set; } = false;

}
