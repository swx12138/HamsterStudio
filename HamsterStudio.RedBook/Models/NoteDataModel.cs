using HamsterStudio.RedBook.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models;

public class NoteDataModel
{
    [JsonPropertyName("firstNoteId")]
    public string FirstNoteId { get; set; } = string.Empty;

    [JsonPropertyName("currentNoteId")]
    public string CurrentNoteId { get; set; } = string.Empty;

    [JsonPropertyName("noteDetailMap")]
    public Dictionary<string, NoteDetailMapModel> NoteDetailMap { get; set; } = [];

    [JsonPropertyName("serverRequestInfo")]
    public ServerRequestInfoModel ServerRequestInfo { get; set; } = new();
}