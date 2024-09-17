using System.Collections.Concurrent;

namespace LibraryCore.IntegrationTests.Framework.Kafka.Services;

public record KafkaTopic1MessagePayload(Guid Id, Guid TestId, string Topic, int? NodeId, string Message);

public class MockDatabase
{
    private ConcurrentBag<KafkaTopic1MessagePayload> DatabaseValues { get; } = [];

    public void AddRecord(KafkaTopic1MessagePayload record, int nodeId)
    {
        DatabaseValues.Add(record with { NodeId = nodeId });
    }

    public IEnumerable<KafkaTopic1MessagePayload> GetMessages() => DatabaseValues;

    public IEnumerable<int> DistinctNodes() => DatabaseValues.Where(x => x.NodeId.HasValue).Select(x => x.NodeId.GetValueOrDefault()).Distinct();
}