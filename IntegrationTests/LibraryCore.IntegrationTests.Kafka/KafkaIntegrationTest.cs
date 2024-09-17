using LibraryCore.Core.DiagnosticUtilities;
using LibraryCore.IntegrationTests.Framework.Kafka.Registration;
using LibraryCore.IntegrationTests.Kafka.Fixtures;
using System.Net.Http.Json;

namespace LibraryCore.IntegrationTests.Kafka;

public class KafkaIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture) : IClassFixture<WebApplicationFactoryFixture>
{
    private WebApplicationFactoryFixture WebApplicationFactoryFixture { get; } = webApplicationFactoryFixture;

    public record ResponseProcessedItem(Guid Id, Guid TestId, string Topic, int? NodeId, string Message);
    public record RequestPublishModel(string Topic, Guid TestId, Guid KeyId, string Message);

    [Fact(Skip = WebApplicationFactoryFixture.SkipReason)]
    public async Task FullIntegrationTest()
    {
        const int howManyRecordsToInsert = 10;
        var testId = Guid.NewGuid();
        var topicsToTestWith = KafkaRegistration.TopicsToUse.Single();

        var messagesToPublish = Enumerable.Range(0, howManyRecordsToInsert)
                                .Select(i => new RequestPublishModel(topicsToTestWith, testId, Guid.NewGuid(), $"message{i}"))
                                .ToList();

        _ = (await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("kafka", messagesToPublish)).EnsureSuccessStatusCode();

        //give it some time to get setup
        await Task.Delay(TimeSpan.FromMinutes(1));

        //try to wait until the test passes...Or kill it after x number of seconds
        var spinWaitResult = await DiagnosticUtility.SpinUntilAsync(async () =>
        {
            return howManyRecordsToInsert == await WebApplicationFactoryFixture.HttpClientToUse.GetFromJsonAsync<int>($"kafkaMessageCount?TestId={testId}");

        }, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));

        //did it find all the records (criteria of the spin until)
        Assert.True(spinWaitResult, "Spin Until Messages Were Received Failed");

        //go grab the actual records so we can test with whats inside
        var recordsFound = await WebApplicationFactoryFixture.HttpClientToUse.GetFromJsonAsync<IEnumerable<ResponseProcessedItem>>($"kafka?TestId={testId}") ?? throw new Exception("Value Can't Be Deserialized");

        Assert.Equal(howManyRecordsToInsert, recordsFound.Count());

        //this should be spread out round robin. If this fails its not an "error"...but should be looked into why its not spreading the messages out
        Assert.True(recordsFound.GroupBy(x => x.NodeId).Count() >= 2);

        //make sure we have the right messages
        foreach (var toPublish in messagesToPublish)
        {
            Assert.Contains(recordsFound, x => x.Topic == toPublish.Topic && x.TestId == testId && x.Id == toPublish.KeyId && x.Message == toPublish.Message);
        }

        //let it hang out for a few seconds to ensure the threads keep going and processes more.
        await Task.Delay(TimeSpan.FromSeconds(15));

        var newTestId = Guid.NewGuid();

        //push 2 more records
        (await WebApplicationFactoryFixture.HttpClientToUse.PostAsJsonAsync("kafka", new List<RequestPublishModel>
        {
            new(topicsToTestWith,newTestId, Guid.NewGuid(), "Message100"),
            new(topicsToTestWith,newTestId, Guid.NewGuid(), "Message101")
        })).EnsureSuccessStatusCode();

        var spinWaitResult2 = await DiagnosticUtility.SpinUntilAsync(async () =>
        {
            return 2 == await WebApplicationFactoryFixture.HttpClientToUse.GetFromJsonAsync<int>($"kafkaMessageCount?TestId={newTestId}");

        }, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30));

        Assert.True(spinWaitResult2);
    }
}
