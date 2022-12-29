using Confluent.Kafka;
using LibraryCore.Core.DiagnosticUtilities;
using LibraryCore.IntegrationTests.Framework.Kafka.Registration;
using LibraryCore.IntegrationTests.Kafka.Fixtures;
using System.Diagnostics;
using System.Net.Http.Json;

namespace LibraryCore.IntegrationTests.Kafka;

public class KafkaIntegrationTest : IClassFixture<WebApplicationFactoryFixture>
{
    public KafkaIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture)
    {
        WebApplicationFactoryFixture = webApplicationFactoryFixture;
    }

    private WebApplicationFactoryFixture WebApplicationFactoryFixture { get; }

    public record ProcessedItem(string Topic, int NodeId, string Key, PublishModel Value);
    public record PublishModel(string Topic, Guid TestId, string KeyId, string Message);

    [Fact(Skip = WebApplicationFactoryFixture.SkipReason)]
    public async Task FullIntegrationTest()
    {
        var testId = Guid.NewGuid();
        const int howManyRecordsToInsert = 10;
        var topicsToTestWith = KafkaRegistration.TopicsToUse.Single();

        var messagesToPublish = new List<PublishModel>();

        for (int i = 0; i < howManyRecordsToInsert; i++)
        {
            messagesToPublish.Add(new PublishModel(topicsToTestWith, testId, i.ToString(), $"message{i}"));
        }

        _ = (await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("kafka", messagesToPublish)).EnsureSuccessStatusCode();

        await Task.Delay(2000);

        //try to wait until the test passes...Or kill it after 5 seconds
        var spinWaitResult = await DiagnosticUtility.SpinUntilAsync(async () =>
        {
            return howManyRecordsToInsert == await WebApplicationFactoryFixture.HttpClientToUse.GetFromJsonAsync<int>($"kafkaMessageCount?TestId={testId}");

        }, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(60));

        //did it find all the records
        Assert.True(spinWaitResult);

        var recordsFound = await WebApplicationFactoryFixture.HttpClientToUse.GetFromJsonAsync<IEnumerable<ProcessedItem>>($"kafka?TestId={testId}") ?? throw new Exception("Value Can't Be Deserialized");

        Assert.Equal(howManyRecordsToInsert, recordsFound.Count());

        //this should be spread out round robin. If this fails its not an "error"...but should be looked into why its not spreading the messages out
        Assert.Equal(2, recordsFound.GroupBy(x => x.NodeId).Count());

        foreach (var toPublish in messagesToPublish)
        {
             Assert.Contains(recordsFound, x => x.Topic == toPublish.Topic && x.Key == toPublish.KeyId && x.Value.Message == toPublish.Message);
        }
    }
}
