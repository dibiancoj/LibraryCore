using Confluent.Kafka;
using LibraryCore.Kafka;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Tests.Kafka.Framework;

[ExcludeFromCodeCoverage(Justification = "Coming up in code coverage report as actual code.")]
public class MyUnitTestHostedAgent : IKafkaProcessor<string, string>
{
    public MyUnitTestHostedAgent(IConsumer<string, string> consumer, HowManyNodesSetup howManyNodesSetup)
    {
        KafkaConsumer = consumer;
        MessagesProcessed = new ConcurrentBag<ProcessedItem>();
        TopicsToRead = new[] { "Topic1", "Topic2" };
        NodeCount = howManyNodesSetup.HowManyNodes;
    }

    public ConcurrentBag<ProcessedItem> MessagesProcessed { get; }

    public IConsumer<string, string> KafkaConsumer { get; }

    public IEnumerable<string> TopicsToRead { get; }

    public int NodeCount { get; }

    public async Task ProcessMessageAsync(ConsumeResult<string, string> messageResult, int nodeId, CancellationToken stoppingToken)
    {
        MessagesProcessed.Add(new ProcessedItem(messageResult.Topic, nodeId, messageResult.Message));

        //throw in a wait for simulation
        await Task.Delay(50, stoppingToken);
    }

    public record ProcessedItem(string Topic, int NodeId, Message<string, string> Message);

    public class HowManyNodesSetup
    {
        public HowManyNodesSetup(int howManyNodes)
        {
            HowManyNodes = howManyNodes;
        }

        public int HowManyNodes { get; }
    }
}