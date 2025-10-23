using HamsterStudio.RedBook.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models;

public class PcWebPostBodyModel
{
    [JsonPropertyName("detail")]
    public NoteDetailModel NoteDetail { get; set; } = new();

    [JsonPropertyName("options")]
    public NoteDataOptionsModel Options { get; set; } = new();

    [JsonPropertyName("comments")]
    public CommentModel[] Comments { get; set; } = [];

}