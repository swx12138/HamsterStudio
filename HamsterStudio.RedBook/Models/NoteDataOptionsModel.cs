using HamsterStudio.RedBook.Models.Sub;
using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models;

public class NoteDataOptionsModel
{
    [JsonPropertyName("with_comments")]
    public bool WithComments { get; set; } = true;

    [JsonPropertyName("author_comments_only")]
    public bool AuthorCommentsOnly { get; set; } = true;

}

public class CommentModel
{
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    public string[] ImageList { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("sub")]
    public CommentModel[] Sub { get; set; } = [];

}
