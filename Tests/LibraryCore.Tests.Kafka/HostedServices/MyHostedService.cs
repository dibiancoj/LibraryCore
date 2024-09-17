using Confluent.Kafka;
using LibraryCore.Kafka.HostedServices;
using LibraryCore.Kafka.Models;
using LibraryCore.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LibraryCore.Tests.Kafka.HostedServices;

public record MyMessage(Guid Id, string Description);

public class MyHostedService(ILogger<KafkaConsumerService<MyMessage>> logger, IOptions<KafkaAppSettings> kafkaAppSettings, int numberOfNodes) : KafkaConsumerService<MyMessage>
    (logger, kafkaAppSettings)
{
    public override string TopicToConsumeFrom => "MyTopic";

    public override string ConsumerGroup => "MyConsumerGroup";

    public override int MinimumNumberOfNodes { get; } = numberOfNodes;

    public override TimeSpan NoMessageBackOffPeriod => TimeSpan.FromSeconds(1);

    public ConcurrentDictionary<Guid, (MyMessage Message, int NodeId, bool IsCommitted)> MyDatabase { get; } = new();

    protected override Task ProcessMessageAsync(IConsumer<string, KafkaNullableOfT<MyMessage>> consumer, ConsumeResult<string, KafkaNullableOfT<MyMessage>> consumeResult, int node)
    {
        if (consumeResult.Message.Value.TryGetResult(out var tryGetModel))
        {
            MyDatabase.TryAdd(tryGetModel.Id, (tryGetModel, node, false));
        }

        return Task.CompletedTask;
    }

    protected override ValueTask MarkMessageAsProcessedAsync(IConsumer<string, KafkaNullableOfT<MyMessage>> consumer,
                                                             ConsumeResult<string, KafkaNullableOfT<MyMessage>> consumeResult,
                                                             int node)
    {
        if (consumeResult.Message.Value.TryGetResult(out var tryGetModel))
        {
            MyDatabase[tryGetModel.Id] = (tryGetModel, node, true);
        }

        consumer.StoreOffset(consumeResult);

        return ValueTask.CompletedTask;
    }
}
