using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Services;
using LibraryCore.IntegrationTests.Framework.Kafka.Settings;
using LibraryCore.Kafka.HostedServices;
using LibraryCore.Kafka.Models;
using Microsoft.Extensions.Options;

namespace LibraryCore.IntegrationTests.Framework.Kafka.HostedAgents;

public class Topic1HostedService(MockDatabase mockDatabase, ILogger<KafkaConsumerService<KafkaTopic1MessagePayload>> logger, IOptions<SampleKafkaSettings> kafkaAppSettings)
    : KafkaConsumerService<KafkaTopic1MessagePayload>(logger, kafkaAppSettings)
{
    private static Guid ConsumerGroupAppend { get; } = Guid.NewGuid();

    public override int MinimumNumberOfNodes => kafkaAppSettings.Value.MinimumNumberOfNodes;

    public override string TopicToConsumeFrom => kafkaAppSettings.Value.Topic;

    public override string ConsumerGroup => $"{kafkaAppSettings.Value.ConsumerGroup}-{ConsumerGroupAppend}";

    public override AutoOffsetReset OffsetReset => AutoOffsetReset.Earliest;

    private MockDatabase Database { get; } = mockDatabase;

    protected override Task ProcessMessageAsync(IConsumer<string, KafkaNullableOfT<KafkaTopic1MessagePayload>> consumer,
                                                ConsumeResult<string, KafkaNullableOfT<KafkaTopic1MessagePayload>> consumeResult,
                                                int nodeId)
    {
        if (!consumeResult.Message.Value.TryGetResult(out var tryGetMessage))
        {
            Logger.LogInformation("Received Message Not Deserialized. NodeId = {NodeId}", nodeId);
        }
        else
        {
            Logger.LogInformation("Received Message Id = {Id}. NodeId = {NodeId}", tryGetMessage.Id, nodeId);
            Database.AddRecord(tryGetMessage, nodeId);
        }

        return Task.CompletedTask;
    }
}