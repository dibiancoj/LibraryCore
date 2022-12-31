using LibraryCore.Core.DiagnosticUtilities;
using LibraryCore.IntegrationTests.Framework.Kafka.Registration;
using LibraryCore.IntegrationTests.Kafka.Fixtures;
using System.Net.Http.Json;

namespace LibraryCore.IntegrationTests.Kafka;

public class KafkaIntegrationTest : IClassFixture<WebApplicationFactoryFixture>
{
    public KafkaIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture)
    {
        WebApplicationFactoryFixture = webApplicationFactoryFixture;
    }

    private WebApplicationFactoryFixture WebApplicationFactoryFixture { get; }

    public record ResponseProcessedItem(string Topic, Guid TestId, int NodeId, string KeyId, string Message);
    public record RequestPublishModel(string Topic, Guid TestId, string KeyId, string Message);

    [Fact(Skip = WebApplicationFactoryFixture.SkipReason)]
    public async Task FullIntegrationTest()
    {
        const int howManyRecordsToInsert = 10;
        var testId = Guid.NewGuid();
        var topicsToTestWith = KafkaRegistration.TopicsToUse.Single();

        var messagesToPublish = new List<RequestPublishModel>();

        for (int i = 0; i < howManyRecordsToInsert; i++)
        {
            messagesToPublish.Add(new RequestPublishModel(topicsToTestWith, testId, i.ToString(), $"message{i}"));
        }

        _ = (await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("kafka", messagesToPublish)).EnsureSuccessStatusCode();

        //give it some time to get setup
        await Task.Delay(1000);

        //try to wait until the test passes...Or kill it after x number of seconds
        var spinWaitResult = await DiagnosticUtility.SpinUntilAsync(async () =>
        {
            return howManyRecordsToInsert == await WebApplicationFactoryFixture.HttpClientToUse.GetFromJsonAsync<int>($"kafkaMessageCount?TestId={testId}");

        }, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(60));

        //did it find all the records (criteria of the spin until)
        Assert.True(spinWaitResult, "Spin Until Messages Were Received Failed");

        //go grab the actual records so we can test with whats inside
        var recordsFound = await WebApplicationFactoryFixture.HttpClientToUse.GetFromJsonAsync<IEnumerable<ResponseProcessedItem>>($"kafka?TestId={testId}") ?? throw new Exception("Value Can't Be Deserialized");

        Assert.Equal(howManyRecordsToInsert, recordsFound.Count());

        //this should be spread out round robin. If this fails its not an "error"...but should be looked into why its not spreading the messages out
        Assert.True(recordsFound.GroupBy(x => x.NodeId).Count() > 2);

        //make sure we have the right messages
        foreach (var toPublish in messagesToPublish)
        {
            Assert.Contains(recordsFound, x => x.Topic == toPublish.Topic && x.TestId == testId && x.KeyId == toPublish.KeyId && x.Message == toPublish.Message);
        }
    }
}
