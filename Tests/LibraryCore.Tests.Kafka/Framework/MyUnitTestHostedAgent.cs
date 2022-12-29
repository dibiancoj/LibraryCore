using Confluent.Kafka;
using LibraryCore.Kafka;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Tests.Kafka.Framework;

[ExcludeFromCodeCoverage(Justification = "Coming up in code coverage report as actual code.")]
public class MyUnitTestHostedAgent : IKafkaProcessor<string, string>
{
    public MyUnitTestHostedAgent(IConsumer<string, string> consumer)
    {
        KafkaConsumer = consumer;
        MessagesProcessed = new ConcurrentBag<ProcessedItem>();
        TopicsToRead = new[] { "Topic1", "Topic2" };
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