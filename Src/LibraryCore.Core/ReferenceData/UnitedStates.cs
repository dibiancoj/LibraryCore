using LibraryCore.Core.Properties;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Core.ReferenceData;

public static class UnitedStates
{
    private record TempStateStorageModel([property: JsonPropertyName("states")] IEnumerable<UnitedStatesStateModel> States);

    [DebuggerDisplay("Id = {Id} | Description = {Description}")]
    public record UnitedStatesStateModel([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("description")] string Description);

    [RequiresUnreferencedCode("DynamicBehavior is incompatible with trimming.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("DynamicBehavior is incompatible with trimming.")]
#endif
    public static IEnumerable<UnitedStatesStateModel> StateListing()
    {
        return JsonSerializer.Deserialize<TempStateStorageModel>(Resources.UnitedState_StateListing)!.States;
    }
}
