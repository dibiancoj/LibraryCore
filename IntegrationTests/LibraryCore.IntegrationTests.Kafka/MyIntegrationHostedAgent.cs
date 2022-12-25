using Confluent.Kafka;
using LibraryCore.Kafka;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LibraryCore.IntegrationTests.Kafka;

public class MyIntegrationHostedAgent : KafkaConsumerService<string, string>
{
    public MyIntegrationHostedAgent(ILogger<KafkaConsumerService<string, string>> logger, IConsumer<string, string> consumer)
        : base(logger, consumer)
    {
        MessagesProcessed = new ConcurrentDictionary<string, string>();
    }

    public IDictionary<string, string> MessagesProcessed { get; }

    protected override IEnumerable<string> TopicsToRead => new[] { "Topic1" };

    protected override int NumberOfReaders => 2;

    protected override async Task ProcessMessageAsync(ConsumeResult<string, string> messageResult, int nodeIndex, CancellationToken stoppingToken)
    {
        MessagesProcessed.Add(messageResult.Message.Key, messageResult.Message.Value);

        //throw in a wait for simulation
        await Task.Delay(50, stoppingToken);
    }
}
