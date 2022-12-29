using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Registration;
using LibraryCore.Kafka;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using static LibraryCore.IntegrationTests.Framework.Kafka.Api.KafkaApi;
using static LibraryCore.IntegrationTests.Framework.Kafka.MyIntegrationHostedAgentMockDatabase;

namespace LibraryCore.IntegrationTests.Framework.Kafka;

[ExcludeFromCodeCoverage(Justification = "Coming up in code coverage report as actual code.")]
public class MyIntegrationHostedAgent : IKafkaProcessor<string, PublishModel>
{
    public MyIntegrationHostedAgent(IConsumer<string, PublishModel> kafkaConsumer, MyIntegrationHostedAgentMockDatabase myIntegrationHostedAgentMockDatabase)
    {
        KafkaConsumer = kafkaConsumer;
        MyIntegrationHostedAgentMockDatabase = myIntegrationHostedAgentMockDatabase;
        TopicsToRead = KafkaRegistration.TopicsToUse;
    }

    public IConsumer<string, PublishModel> KafkaConsumer { get; }
    private MyIntegrationHostedAgentMockDatabase MyIntegrationHostedAgentMockDatabase { get; }
    public IEnumerable<string> TopicsToRead { get; }

    public async Task ProcessMessageAsync(ConsumeResult<string, PublishModel> messageResult, int nodeId, CancellationToken stoppingToken)
    {
        MyIntegrationHostedAgentMockDatabase.MessagesProcessed.Add(new ProcessedItem(messageResult.Topic, nodeId, messageResult.Message.Key, messageResult.Message.Value));

        //throw in a wait for simulation
        await Task.Delay(250, stoppingToken);
    }
}

public class MyIntegrationHostedAgentMockDatabase
{
    public MyIntegrationHostedAgentMockDatabase()
    {
        MessagesProcessed = new ConcurrentBag<ProcessedItem>();
    }

    public record ProcessedItem(string Topic, int NodeId, string Key, PublishModel Value);

    public ConcurrentBag<ProcessedItem> MessagesProcessed { get; }
}
