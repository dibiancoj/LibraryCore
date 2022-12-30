namespace LibraryCore.IntegrationTests.Framework.Kafka.Api.Models;

public record RequestPublishModel(string Topic, Guid TestId, string KeyId, string Message);