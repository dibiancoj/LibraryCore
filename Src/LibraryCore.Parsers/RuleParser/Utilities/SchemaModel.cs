using LibraryCore.Core.EnumUtilities;
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
        [SchemaDataTypeDotNetTypeAttibute(typeof(int))]
        Int = 0,

        [SchemaDataTypeDotNetTypeAttibute(typeof(string))]
        String = 1,

        [SchemaDataTypeDotNetTypeAttibute(typeof(bool))]
        Boolean = 2,

        [SchemaDataTypeDotNetTypeAttibute(typeof(DateTime))]
        DateTime = 3,

        [SchemaDataTypeDotNetTypeAttibute(typeof(IEnumerable<int>))]
        ArrayOfInts,

        [SchemaDataTypeDotNetTypeAttibute(typeof(IEnumerable<string>))]
        ArrayOfStrings,
    }

    private class SchemaDataTypeDotNetTypeAttibute : Attribute
    {
        public SchemaDataTypeDotNetTypeAttibute(Type dotNetType)
        {
            DotNetType = dotNetType;
        }

        public Type DotNetType { get; }
    }

    internal static readonly MethodInfo JsonElementGetProperty = JsonElementGetMethodInfoHelper("GetProperty", typeof(string));

    private static Dictionary<SchemaDataType, MethodInfo> DeserializeToTypeCache { get; } = DeserializeToTypeCacheBuilder().ToDictionary(x => x.SchemaType, x => x.MethodInfo);

    private static MethodInfo JsonElementGetMethodInfoHelper(string methodName, params Type[] types) =>
        typeof(JsonElement).GetMethod(methodName, types) ?? throw new Exception($"Can't Find {methodName} In {nameof(JsonElementGetMethodInfoHelper)}");

    public static MethodInfo MethodInfoToConvertValue(SchemaDataType schemaValue) => DeserializeToTypeCache[schemaValue];

    private static IEnumerable<(SchemaDataType SchemaType, MethodInfo MethodInfo)> DeserializeToTypeCacheBuilder()
    {
        //in an effort not to have specific types array, int, string, datetime...just going to deserialize the property.
        //Can cache this is performance becomes a problem with the basic types
        var temp = typeof(JsonSerializer).GetMethod(nameof(JsonSerializer.Deserialize),
                                                new[] { typeof(JsonElement),
                                                        typeof(JsonSerializerOptions)
                                                }) ?? throw new Exception("Can't Find Method To Convert Dynamic Type");

        foreach (var enumValue in EnumUtility.GetValuesLazy<SchemaDataType>())
        {
            yield return (enumValue, temp.MakeGenericMethod(EnumUtility.CustomAttributeGet<SchemaDataTypeDotNetTypeAttibute>(enumValue).DotNetType));
        }
    }
}
