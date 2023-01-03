using Confluent.Kafka;

namespace LibraryCore.Kafka;

public interface IKafkaProcessor<TKafkaKey, TKafkaMessageBody>
{
    public IConsumer<TKafkaKey, TKafkaMessageBody> KafkaConsumer { get; }
    public IEnumerable<string> TopicsToRead { get; }
    public TimeSpan KafkaConsumeTimeOut() => TimeSpan.FromSeconds(15);
    public int NodeCount { get; }
    public Task ProcessMessageAsync(ConsumeResult<TKafkaKey, TKafkaMessageBody> messageResult, int nodeId, CancellationToken stoppingToken);
}
