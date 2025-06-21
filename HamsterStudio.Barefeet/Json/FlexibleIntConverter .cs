using System.Text.Json;
using System.Text.Json.Serialization;

namespace HamsterStudio.Barefeet.Json;

public class FlexibleIntConverter : JsonConverter<int>
{
    public override int Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetInt32();

            case JsonTokenType.String:
                if (int.TryParse(reader.GetString(), out int result))
                    return result;
                break;
        }

        throw new JsonException($"Expected number or numeric string, got {reader.TokenType}");
    }

    public override void Write(
        Utf8JsonWriter writer,
        int value,
        JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
