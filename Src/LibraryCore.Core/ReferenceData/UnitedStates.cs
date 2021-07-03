using LibraryCore.Core.Properties;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Core.ReferenceData
{
    public static class UnitedStates
    {
        private record TempStateStorageModel
        {
            [JsonPropertyName("states")]
            public IEnumerable<UnitedStatesStateModel> States { get; init; } = null!;
        }

        public record UnitedStatesStateModel
        {
            [JsonPropertyName("id")]
            public string Id { get; init; } = null!;

            [JsonPropertyName("description")]
            public string Description { get; init; } = null!;
        }

        public static IEnumerable<UnitedStatesStateModel> StateListing()
        {
            return JsonSerializer.Deserialize<TempStateStorageModel>(Resources.UnitedState_StateListing)?.States ?? throw new Exception("Can't Deserialize UnitedStates.StateListing");
        }
    }
}
