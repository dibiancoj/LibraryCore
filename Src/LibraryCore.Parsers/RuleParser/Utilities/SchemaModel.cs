using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Parsers.RuleParser.Utilities;

public record SchemaModel(JsonElement? Schema)
{
    public static SchemaModel Create(Func<object>? schemaModel) => new(schemaModel == null ? null : JsonSerializer.SerializeToElement(schemaModel()));

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SchemaDataType
    {
        Int = 0,
        String = 1,
        Boolean = 2,
        DateTime = 3
    }

    public static MethodInfo MethodInfoToConvertValue(SchemaDataType schemaValue) =>
           schemaValue switch
           {
               SchemaDataType.Int => typeof(JsonElement).GetMethod("GetInt32") ?? throw new Exception("Can't Find GetInt32 In Dynamic"),
               SchemaDataType.Boolean => typeof(JsonElement).GetMethod("GetBoolean") ?? throw new Exception("Can't Find GetBoolean In Dynamic"),
               SchemaDataType.DateTime => typeof(JsonElement).GetMethod("GetDateTime") ?? throw new Exception("Can't Find GetDateTime In Dynamic"),
               SchemaDataType.String => typeof(JsonElement).GetMethod("GetString") ?? throw new Exception("Can't Find GetString In Dynamic"),
               _ => throw new ArgumentException("Invalid Enum With Dynamic Method Call")
           };
}
