using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Api.Models;
using LibraryCore.Kafka;
using System.Collections.Concurrent;

namespace LibraryCore.IntegrationTests.Framework.Kafka;

public class MyHostedAgent : BackgroundService
{
    public MyHostedAgent(KafkaNodeManager kafkaNodeManager)
    {
        KafkaNodeManager = kafkaNodeManager;
    }

    private KafkaNodeManager KafkaNodeManager { get; }
    public const string KakfaJobName = "Job1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(KafkaNodeManager.CreateNodeAsync(KakfaJobName, stoppingToken));
    }
}

public class MyKafkaJob1 : KafkaNode<string, KafkaMessageModel>
{
    public MyKafkaJob1(ILogger<MyKafkaJob1> logger, IEnumerable<string> topicsToRead, IConsumer<string, KafkaMessageModel> kafkaConsumer, MyIntegrationHostedAgentMockDatabase myIntegrationHostedAgentMockDatabase) :
        base(logger, topicsToRead, kafkaConsumer)
    {
        MyIntegrationHostedAgentMockDatabase = myIntegrationHostedAgentMockDatabase;
    }

    public MyIntegrationHostedAgentMockDatabase MyIntegrationHostedAgentMockDatabase { get; }

    public override async Task ProcessMessageAsync(ConsumeResult<string, KafkaMessageModel> messageResult, int nodeId, CancellationToken stoppingToken)
    {
        MyIntegrationHostedAgentMockDatabase.MessagesProcessed.Add(new KafkaProcessedItemModel(messageResult.Topic, messageResult.Message.Value.TestId, nodeId, messageResult.Message.Key, messageResult.Message.Value.Message));

        await Task.CompletedTask;
    }
}

//[ExcludeFromCodeCoverage(Justification = "Coming up in code coverage report as actual code.")]
//public class MyIntegrationHostedAgent : IKafkaProcessor<string, KafkaMessageModel>
//{
//    public MyIntegrationHostedAgent(IConsumer<string, KafkaMessageModel> kafkaConsumer, MyIntegrationHostedAgentMockDatabase myIntegrationHostedAgentMockDatabase)
//    {
//        KafkaConsumer = kafkaConsumer;
//        MyIntegrationHostedAgentMockDatabase = myIntegrationHostedAgentMockDatabase;
//        TopicsToRead = KafkaRegistration.TopicsToUse;
//    }

//    public IConsumer<string, KafkaMessageModel> KafkaConsumer { get; }
//    private MyIntegrationHostedAgentMockDatabase MyIntegrationHostedAgentMockDatabase { get; }
//    public IEnumerable<string> TopicsToRead { get; }
//    public int NodeCount => 5;

//    public async Task ProcessMessageAsync(ConsumeResult<string, KafkaMessageModel> messageResult, int nodeId, CancellationToken stoppingToken)
//    {
//        MyIntegrationHostedAgentMockDatabase.MessagesProcessed.Add(new KafkaProcessedItemModel(messageResult.Topic, messageResult.Message.Value.TestId, nodeId, messageResult.Message.Key, messageResult.Message.Value.Message));

//        await Task.CompletedTask;
//    }
//}

public class MyIntegrationHostedAgentMockDatabase
{
    public MyIntegrationHostedAgentMockDatabase()
    {
        MessagesProcessed = new ConcurrentBag<KafkaProcessedItemModel>();
    }

    public ConcurrentBag<KafkaProcessedItemModel> MessagesProcessed { get; }
}
