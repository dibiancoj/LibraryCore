using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Api.Models;
using LibraryCore.IntegrationTests.Framework.Kafka.KafkaProcessor;
using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.IntegrationTests.Framework.Kafka.Api;

public static class KafkaApi
{
    public static IEnumerable<KafkaProcessedItemModel> Get([FromServices] MyIntegrationHostedAgentMockDatabase myIntegrationHostedAgentMockDatabase, [FromQuery] Guid TestId)
    {
        return myIntegrationHostedAgentMockDatabase.MessagesProcessed.Where(x => x.TestId == TestId);
    }

    public static int GetCount([FromServices] MyIntegrationHostedAgentMockDatabase myIntegrationHostedAgentMockDatabase, [FromQuery] Guid TestId)
    {
        return myIntegrationHostedAgentMockDatabase.MessagesProcessed.Count(x => x.TestId == TestId);
    }

    public static async Task PostAsync([FromServices] IProducer<string, KafkaMessageModel> producer, [FromBody] IEnumerable<RequestPublishModel> recordsToPublish)
    {
        foreach (var record in recordsToPublish)
        {
            await producer.ProduceAsync(record.Topic, new Message<string, KafkaMessageModel> { Key = record.KeyId, Value = new KafkaMessageModel(record.TestId, record.Message) });
        }

        producer.Flush();
    }

}
