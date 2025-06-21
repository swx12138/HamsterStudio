using HamsterStudio.Barefeet.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HamsterStudio.Barefeet.Json;

public class FlexibleDoubleConverter : JsonConverter<double>
{
    public override double Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetDouble();

            case JsonTokenType.String:
                string? raw = reader.GetString();
                if (raw?.IsNullOrEmpty() ?? false) return 0.0;
                if (int.TryParse(raw, out int result))
                {
                    return result;
                }
                else
                {

                }
                break;
        }

        throw new JsonException($"Expected number or numeric string, got {reader.TokenType}");
    }

    public override void Write(
        Utf8JsonWriter writer,
        double value,
        JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
