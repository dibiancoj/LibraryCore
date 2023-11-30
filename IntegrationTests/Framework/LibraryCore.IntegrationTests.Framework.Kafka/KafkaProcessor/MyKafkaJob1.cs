using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Api.Models;
using LibraryCore.Kafka;
using System.Collections.Concurrent;

namespace LibraryCore.IntegrationTests.Framework.Kafka.KafkaProcessor;

public class MyKafkaJob1(ILogger<MyKafkaJob1> logger, IEnumerable<string> topicsToRead, IConsumer<string, KafkaMessageModel> kafkaConsumer, MyIntegrationHostedAgentMockDatabase myIntegrationHostedAgentMockDatabase) : KafkaNode<string, KafkaMessageModel>(logger, topicsToRead, kafkaConsumer)
{
    public MyIntegrationHostedAgentMockDatabase MyIntegrationHostedAgentMockDatabase { get; } = myIntegrationHostedAgentMockDatabase;

    public override async Task ProcessMessageAsync(ConsumeResult<string, KafkaMessageModel> messageResult, int nodeId, CancellationToken stoppingToken)
    {
        MyIntegrationHostedAgentMockDatabase.MessagesProcessed.Add(new KafkaProcessedItemModel(messageResult.Topic, messageResult.Message.Value.TestId, nodeId, messageResult.Message.Key, messageResult.Message.Value.Message));

        await Task.CompletedTask;
    }
}

public class MyIntegrationHostedAgentMockDatabase
{
    public MyIntegrationHostedAgentMockDatabase()
    {
        MessagesProcessed = new ConcurrentBag<KafkaProcessedItemModel>();
    }

    public ConcurrentBag<KafkaProcessedItemModel> MessagesProcessed { get; }
}
