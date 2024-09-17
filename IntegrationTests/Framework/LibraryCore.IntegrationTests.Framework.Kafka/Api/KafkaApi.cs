using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Api.Models;
using LibraryCore.IntegrationTests.Framework.Kafka.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryCore.IntegrationTests.Framework.Kafka.Api;

public static class KafkaApi
{
    public static IEnumerable<KafkaTopic1MessagePayload> Get([FromServices] MockDatabase myIntegrationHostedAgentMockDatabase, [FromQuery] Guid TestId)
    {
        return myIntegrationHostedAgentMockDatabase.GetMessages().Where(x => x.TestId == TestId);
    }

    public static int GetCount([FromServices] MockDatabase myIntegrationHostedAgentMockDatabase, [FromQuery] Guid TestId)
    {
        return myIntegrationHostedAgentMockDatabase.GetMessages().Count(x => x.TestId == TestId);
    }

    public static async Task PostAsync([FromServices] IProducer<string, KafkaTopic1MessagePayload> producer, [FromBody] IEnumerable<RequestPublishModel> recordsToPublish)
    {
        foreach (var record in recordsToPublish)
        {
            var messageToPublish = new KafkaTopic1MessagePayload(record.KeyId, record.TestId, record.Topic, null, record.Message);

            await producer.ProduceAsync(record.Topic, new Message<string, KafkaTopic1MessagePayload> { Key = record.KeyId.ToString(), Value = messageToPublish });
        }

        producer.Flush();
    }

}
