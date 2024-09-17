namespace LibraryCore.IntegrationTests.Framework.Kafka.Api.Models;

public record RequestPublishModel(string Topic, Guid TestId, Guid KeyId, string Message);