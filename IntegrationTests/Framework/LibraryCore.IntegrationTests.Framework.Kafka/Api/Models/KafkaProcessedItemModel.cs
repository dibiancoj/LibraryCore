namespace LibraryCore.IntegrationTests.Framework.Kafka.Api.Models;

public record KafkaProcessedItemModel(string Topic, Guid TestId, int NodeId, string KeyId, string Message);
