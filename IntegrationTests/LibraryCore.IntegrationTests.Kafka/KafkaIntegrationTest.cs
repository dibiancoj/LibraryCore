using Confluent.Kafka;
using Confluent.Kafka.Admin;
using LibraryCore.IntegrationTests.Kafka.Fixtures;
using LibraryCore.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryCore.IntegrationTests.Kafka;

public class KafkaIntegrationTest : IClassFixture<KafkaFixture>
{
    public KafkaIntegrationTest(KafkaFixture kafkaFixture)
    {
        KafkaFixture = kafkaFixture;
    }

    private KafkaFixture KafkaFixture { get; }

    [Fact(Skip = KafkaFixture.SkipReason)]
    public async Task FullIntegrationTest()
    {
        var producer = KafkaFixture.Provider.GetRequiredService<IProducer<string, string>>();
        var adminClient = KafkaFixture.Provider.GetRequiredService<IAdminClient>();
        var hostedAgentToTest = KafkaFixture.Provider.GetRequiredService<KafkaConsumerService<string, string>>();
        var processorToTest = (MyIntegrationHostedAgent)KafkaFixture.Provider.GetRequiredService<IKafkaProcessor<string, string>>();

        const string topicNameToUse = KafkaFixture.TopicToTestWith;

        var metaInfo = adminClient.GetMetadata(topicNameToUse, TimeSpan.FromSeconds(10));

        if (!metaInfo.Topics.Any())
        {
            await adminClient.CreateTopicsAsync(new List<TopicSpecification> { new() { Name = topicNameToUse } });
        }

        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        await hostedAgentToTest.StartAsync(cancellationToken.Token);

        for (int i = 0; i < 4; i++)
        {
            await producer.ProduceAsync(topicNameToUse, new Message<string, string> { Key = $"key{i}", Value = $"value{i}" });
        }

        producer.Flush();

        //try to wait until the test passes...Or kill it after 5 seconds
        var spinWaitResult = SpinWait.SpinUntil(() =>
        {
            return processorToTest.MessagesProcessed.Count == 4;

        }, TimeSpan.FromSeconds(30));

        await hostedAgentToTest.StopAsync(cancellationToken.Token);

        //make sure we spun until we found the right amount of records
        Assert.True(spinWaitResult);

        var result = processorToTest.MessagesProcessed;

        Assert.Equal(4, result.Count);
        Assert.Contains(result, x => x.Topic == topicNameToUse && x.Message.Key == "key0" && x.Message.Value == "value0");
        Assert.Contains(result, x => x.Topic == topicNameToUse && x.Message.Key == "key1" && x.Message.Value == "value1");
        Assert.Contains(result, x => x.Topic == topicNameToUse && x.Message.Key == "key2" && x.Message.Value == "value2");
        Assert.Contains(result, x => x.Topic == topicNameToUse && x.Message.Key == "key3" && x.Message.Value == "value3");
    }
}
