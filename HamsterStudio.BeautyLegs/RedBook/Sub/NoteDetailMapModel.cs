using System.Text.Json.Serialization;

namespace HamsterStudio.BeautyLegs.RedBook.Sub
{
    public class NoteDetailMapModel
    {
        [JsonPropertyName("currentTime")]
        public long CurrentTime { get; set; }

        [JsonPropertyName("note")]
        public NoteDetailModel NoteDetail { get; set; } = new();
    }
}
