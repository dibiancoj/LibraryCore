using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using static LibraryCore.IntegrationTests.Framework.Kafka.MyIntegrationHostedAgentMockDatabase;

namespace LibraryCore.IntegrationTests.Framework.Kafka.Api;

public static class KafkaApi
{
    public static IEnumerable<ProcessedItem> Get([FromServices] MyIntegrationHostedAgentMockDatabase myIntegrationHostedAgentMockDatabase, [FromQuery] Guid TestId)
    {
        return myIntegrationHostedAgentMockDatabase.MessagesProcessed.Where(x => x.Value.TestId == TestId);
    }

    public static int GetCount([FromServices] MyIntegrationHostedAgentMockDatabase myIntegrationHostedAgentMockDatabase, [FromQuery] Guid TestId)
    {
        return myIntegrationHostedAgentMockDatabase.MessagesProcessed.Count(x => x.Value.TestId == TestId);
    }

    public record PublishModel(string Topic, Guid TestId, string KeyId, string Message);

    public static async Task PostAsync([FromServices] IProducer<string, PublishModel> producer, [FromBody] IEnumerable<PublishModel> recordsToPublish)
    {
        foreach(var record in recordsToPublish)
        {
            await producer.ProduceAsync(record.Topic, new Message<string, PublishModel> { Key = record.KeyId, Value = record });
        }

        producer.Flush();
    }

}
