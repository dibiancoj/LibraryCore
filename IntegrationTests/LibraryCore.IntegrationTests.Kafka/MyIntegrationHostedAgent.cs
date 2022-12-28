using Confluent.Kafka;
using LibraryCore.IntegrationTests.Kafka.Fixtures;
using LibraryCore.Kafka;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.IntegrationTests.Kafka;

[ExcludeFromCodeCoverage(Justification = "Coming up in code coverage report as actual code.")]
public class MyIntegrationHostedAgent : KafkaConsumerService<string, string>
{
    public MyIntegrationHostedAgent(ILogger<KafkaConsumerService<string, string>> logger, IConsumer<string, string> consumer)
        : base(logger, consumer)
    {
        MessagesProcessed = new ConcurrentBag<ProcessedItem>();
    }

    public ConcurrentBag<ProcessedItem> MessagesProcessed { get; }

    protected override IEnumerable<string> TopicsToRead => new[] { KafkaFixture.TopicToTestWith };

    protected override TimeSpan KafkaConsumeTimeOut => TimeSpan.FromSeconds(3);

    protected override async Task ProcessMessageAsync(ConsumeResult<string, string> messageResult, CancellationToken stoppingToken)
    {
        MessagesProcessed.Add(new ProcessedItem(messageResult.Topic, messageResult.Message));

        //throw in a wait for simulation
        await Task.Delay(50, stoppingToken);
    }

    public record ProcessedItem(string Topic, Message<string, string> Message);
}

