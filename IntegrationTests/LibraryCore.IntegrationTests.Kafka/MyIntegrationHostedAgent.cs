using Confluent.Kafka;
using LibraryCore.IntegrationTests.Kafka.Fixtures;
using LibraryCore.Kafka;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.IntegrationTests.Kafka;

[ExcludeFromCodeCoverage(Justification = "Coming up in code coverage report as actual code.")]
public class MyIntegrationHostedAgent : IKafkaProcessor<string, string>
{
    public MyIntegrationHostedAgent(IConsumer<string, string> kafkaConsumer)
    {
        KafkaConsumer = kafkaConsumer;
        MessagesProcessed = new ConcurrentBag<ProcessedItem>();
        TopicsToRead = new[] { KafkaFixture.TopicToTestWith };
    }

    public ConcurrentBag<ProcessedItem> MessagesProcessed { get; }

    public IConsumer<string, string> KafkaConsumer { get; }

    public IEnumerable<string> TopicsToRead { get; }

    public async Task ProcessMessageAsync(ConsumeResult<string, string> messageResult, CancellationToken stoppingToken)
    {
        MessagesProcessed.Add(new ProcessedItem(messageResult.Topic, messageResult.Message));

        //throw in a wait for simulation
        await Task.Delay(50, stoppingToken);
    }

    public record ProcessedItem(string Topic, Message<string, string> Message);
}

