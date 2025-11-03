using HamsterStudio.RedBook.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models;

public class PcWebPostBodyModel
{
    [JsonPropertyName("detail")]
    public NoteDetailModel NoteDetail { get; set; } = new();

    [JsonPropertyName("noteId")]
    public string NoteId { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public NoteDataOptionsModel Options { get; set; } = new();

    [JsonPropertyName("comments")]
    public CommentsDataModel Comments { get; set; } = new();

}
