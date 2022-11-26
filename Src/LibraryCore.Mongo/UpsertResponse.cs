using MongoDB.Bson;
using System.Text.Json.Serialization;
using static LibraryCore.Mongo.UpsertResponse;

namespace LibraryCore.Mongo;

public record UpsertResponse(ObjectId DocumentId, UpsertModelEnum OperationExecuted)
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UpsertModelEnum
    {
        Inserted,
        Updated
    }
}

public record UpsertManyResponse(IEnumerable<UpsertResponse> Results);