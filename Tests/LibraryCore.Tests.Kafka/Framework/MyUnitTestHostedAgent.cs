using Confluent.Kafka;
using LibraryCore.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Tests.Kafka.Framework;

[ExcludeFromCodeCoverage(Justification = "Coming up in code coverage report as actual code.")]
public class MyUnitTestHostedAgent : BackgroundService
{
    public MyUnitTestHostedAgent(KafkaNodeManager kafkaNodeManager)
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

public class KafkaMockedDataStore
{
    public record ProcessedItem(string Topic, int NodeId, Message<string, string> Message);
    public ConcurrentBag<ProcessedItem> MessagesProcessed { get; } = new ConcurrentBag<ProcessedItem>();
}

public class MyKafkaJob1 : KafkaNode<string, string>
{
    public MyKafkaJob1(ILogger<KafkaNode<string, string>> logger, IEnumerable<string> topicsToRead, IConsumer<string, string> kafkaConsumer, KafkaMockedDataStore kafkaMockedDataStore) :
        base(logger, topicsToRead, kafkaConsumer)
    {
        KafkaMockedDataStore = kafkaMockedDataStore;
    }

    public KafkaMockedDataStore KafkaMockedDataStore { get; }

    public override async Task ProcessMessageAsync(ConsumeResult<string, string> messageResult, int nodeId, CancellationToken stoppingToken)
    {
        KafkaMockedDataStore.MessagesProcessed.Add(new KafkaMockedDataStore.ProcessedItem(messageResult.Topic, nodeId, messageResult.Message));

        //throw in a wait for simulation
        await Task.Delay(50, stoppingToken);
    }
}