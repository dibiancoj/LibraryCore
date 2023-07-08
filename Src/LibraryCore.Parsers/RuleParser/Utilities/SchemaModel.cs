using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Parsers.RuleParser.Utilities;

public record SchemaModel(JsonElement? Schema)
{
    public static SchemaModel Create(object? schemaModel) => new(schemaModel == null ? null : JsonSerializer.SerializeToElement(schemaModel));

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SchemaDataType
    {
        Int = 0,
        String = 1,
        Boolean = 2,
        DateTime = 3
    }

    internal static readonly MethodInfo JsonElementGetProperty = JsonElementGetMethodInfoHelper("GetProperty", typeof(string));
    private static readonly MethodInfo JsonElementGetInt32 = JsonElementGetMethodInfoHelper("GetInt32");
    private static readonly MethodInfo JsonElementGetBoolean = JsonElementGetMethodInfoHelper("GetBoolean");
    private static readonly MethodInfo JsonElementGetDateTime = JsonElementGetMethodInfoHelper("GetDateTime");
    private static readonly MethodInfo JsonElementGetString = JsonElementGetMethodInfoHelper("GetString");

    private static MethodInfo JsonElementGetMethodInfoHelper(string methodName, params Type[] types) =>
        typeof(JsonElement).GetMethod(methodName, types) ?? throw new Exception($"Can't Find {methodName} In {nameof(JsonElementGetMethodInfoHelper)}");

    public static MethodInfo MethodInfoToConvertValue(SchemaDataType schemaValue) =>
           schemaValue switch
           {
               SchemaDataType.Int => JsonElementGetInt32,
               SchemaDataType.Boolean => JsonElementGetBoolean,
               SchemaDataType.DateTime => JsonElementGetDateTime,
               SchemaDataType.String => JsonElementGetString,
               _ => throw new ArgumentException("Invalid Enum With Dynamic Method Call")
           };
}
