namespace LibraryCore.Kafka;

public interface IKafkaNodeCreator
{
    Task CreateNodeAsync(int nodeId, string jobKey, CancellationToken cancellationToken);
}
