using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models.Sub;

public class NoteDetailMapModel
{
    [JsonPropertyName("currentTime")]
    public long CurrentTime { get; set; }

    [JsonPropertyName("note")]
    public NoteDetailModel NoteDetail { get; set; } = new();
}
