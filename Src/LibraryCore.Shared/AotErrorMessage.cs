using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace LibraryCore.Shared;

internal static class ErrorMessages
{
    internal const string AotDynamicAccess = "DynamicBehavior is incompatible with trimming.";
    internal const string AotDynamicAccessUseOverload = "DynamicBehavior is incompatible with trimming. Use the JsonTypeInfo overload for aot support.";
}

internal static class AotUtilities
{
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    internal static JsonTypeInfo<T> ResolveJsonTypeInfo<T>()
    {
        return (JsonTypeInfo<T>)JsonSerializerOptions.Default.GetTypeInfo(typeof(T));
    }
}
