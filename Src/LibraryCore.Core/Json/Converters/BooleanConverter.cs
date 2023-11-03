using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Core.Json.Converters;

/// <summary>
/// Allows us to convert a boolean to something that looks like a string. Boolean's shouldn't have quotes around it
/// </summary>
public class BooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => reader.GetString() switch
            {
                "true" => true,
                "false" => false,
                _ => throw new JsonException()
            },
            _ => throw new JsonException(),
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteBooleanValue(value);
}
