using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Api.Models;
using LibraryCore.IntegrationTests.Framework.Kafka.Registration;
using LibraryCore.Kafka;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.IntegrationTests.Framework.Kafka;

[ExcludeFromCodeCoverage(Justification = "Coming up in code coverage report as actual code.")]
public class MyIntegrationHostedAgent : IKafkaProcessor<string, KafkaMessageModel>
{
    public MyIntegrationHostedAgent(IConsumer<string, KafkaMessageModel> kafkaConsumer, MyIntegrationHostedAgentMockDatabase myIntegrationHostedAgentMockDatabase)
    {
        KafkaConsumer = kafkaConsumer;
        MyIntegrationHostedAgentMockDatabase = myIntegrationHostedAgentMockDatabase;
        TopicsToRead = KafkaRegistration.TopicsToUse;
    }

    public IConsumer<string, KafkaMessageModel> KafkaConsumer { get; }
    private MyIntegrationHostedAgentMockDatabase MyIntegrationHostedAgentMockDatabase { get; }
    public IEnumerable<string> TopicsToRead { get; }

    public async Task ProcessMessageAsync(ConsumeResult<string, KafkaMessageModel> messageResult, int nodeId, CancellationToken stoppingToken)
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
