using System.Text.Json.Serialization;

namespace HamsterStudio.RedBook.Models;

public class NoteDataOptionsModel
{
    [JsonPropertyName("with_comments")]
    public bool WithComments { get; set; } = true;

    [JsonPropertyName("author_comments_only")]
    public bool AuthorCommentsOnly { get; set; } = true;

}
