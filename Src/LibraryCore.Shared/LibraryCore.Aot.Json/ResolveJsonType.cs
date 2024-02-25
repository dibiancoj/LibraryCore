using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using LibraryCore.Shared;

namespace LibraryCore.Aot.Json;

public static class ResolveJsonType
{
    //most of this is taken from System.Net.Http.Json.JsonHelpers to allow non aot to use the same code and pull back json type info using web options.
    //this way camel case works by default
    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http.Json/src/System/Net/Http/Json/JsonHelpers.cs
    private static JsonSerializerOptions? WebDefaultSerializerOptions { get; set; }

    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public static JsonTypeInfo<T> ResolveJsonTypeInfo<T>(JsonSerializerDefaults serializationDefault = JsonSerializerDefaults.Web)
    {
        return (JsonTypeInfo<T>)ResolveOptionsToUse(null, serializationDefault).GetTypeInfo(typeof(T));
    }

    public static JsonTypeInfo<T> ResolveJsonTypeInfo<T>(JsonSerializerOptions? jsonSerializerOptions)
    {
        return (JsonTypeInfo<T>)ResolveOptionsToUse(jsonSerializerOptions, JsonSerializerDefaults.Web).GetTypeInfo(typeof(T));
    }

    private static JsonSerializerOptions ResolveOptionsToUse(JsonSerializerOptions? jsonSerializerOptions, JsonSerializerDefaults serializationDefault)
    {
        if (jsonSerializerOptions is not null)
        {
            return jsonSerializerOptions;
        }

        return serializationDefault == JsonSerializerDefaults.Web ?
                                            (WebDefaultSerializerOptions ??= CreateWebSerializer()) :
                                            JsonSerializerOptions.Default;
    }

    private static JsonSerializerOptions CreateWebSerializer() => new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };
}
