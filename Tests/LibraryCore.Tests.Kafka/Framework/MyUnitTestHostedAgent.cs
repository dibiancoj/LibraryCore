using Confluent.Kafka;
using LibraryCore.Kafka;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LibraryCore.Tests.Kafka.Framework;

public class MyUnitTestHostedAgent : KafkaConsumerService<string, string>
{
    public MyUnitTestHostedAgent(ILogger<KafkaConsumerService<string, string>> logger, IConsumer<string, string> consumer)
        : base(logger, consumer)
    {
        MessagesProcessed = new ConcurrentDictionary<string, (int NodeIndex, string Message)>();
    }

    public IDictionary<string, (int NodeIndex, string Message)> MessagesProcessed { get; }

    protected override IEnumerable<string> TopicsToRead => new[] { "Topic1" };

    protected override int NumberOfReaders => 2;

    protected override TimeSpan KafkaConsumeTimeOut => TimeSpan.FromSeconds(3);

    protected override async Task ProcessMessageAsync(ConsumeResult<string, string> messageResult, int nodeIndex, CancellationToken stoppingToken)
    {
        MessagesProcessed.Add(messageResult.Message.Key, (nodeIndex, messageResult.Message.Value));

        //throw in a wait for simulation
        await Task.Delay(50, stoppingToken);
    }
}

