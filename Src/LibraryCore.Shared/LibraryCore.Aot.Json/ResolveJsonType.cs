using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using LibraryCore.Shared;

namespace LibraryCore.Aot.Json;

public static class ResolveJsonType
{
    static ResolveJsonType()
    {
        WebDefaultSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
    }
    //most of this is taken from System.Net.Http.Json.JsonHelpers to allow non aot to use the same code and pull back json type info using web options.
    //this way camel case works by default
    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http.Json/src/System/Net/Http/Json/JsonHelpers.cs
    private static JsonSerializerOptions WebDefaultSerializerOptions { get; }

    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public static JsonTypeInfo<T> ResolveJsonTypeInfo<T>(JsonSerializerDefaults serializationDefault = JsonSerializerDefaults.Web)
    {
        return serializationDefault == JsonSerializerDefaults.Web ?
            (JsonTypeInfo<T>)WebDefaultSerializerOptions.GetTypeInfo(typeof(T)) :
            (JsonTypeInfo<T>)JsonSerializerOptions.Default.GetTypeInfo(typeof(T));
    }
}
