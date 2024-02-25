using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using LibraryCore.Shared;

namespace LibraryCore.Aot.Json;

public static class ResolveJsonType
{
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public static JsonTypeInfo<T> ResolveJsonTypeInfo<T>()
    {
        return (JsonTypeInfo<T>)JsonSerializerOptions.Default.GetTypeInfo(typeof(T));
    }
}
