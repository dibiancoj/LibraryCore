using Castle.Core.Logging;
using Confluent.Kafka;
using LibraryCore.Kafka;
using LibraryCore.Tests.Kafka.Framework;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Tests.Kafka;

[ExcludeFromCodeCoverage(Justification = "Coming up in code coverage report as actual code.")]
public class KafkaTest
{
    public KafkaTest()
    {
        MockedConsumerKafka = new Mock<IConsumer<string, string>>();

        Provider = new ServiceCollection()
           .AddLogging()
           .AddSingleton<KafkaConsumerService<string, string>>()
           .AddSingleton(MockedConsumerKafka.Object)
           .AddSingleton<IKafkaProcessor<string, string>, MyUnitTestHostedAgent>()
           .BuildServiceProvider();
    }

    private Mock<IConsumer<string, string>> MockedConsumerKafka { get; set; }
    private ServiceProvider Provider { get; }

    [Fact]
    public async Task BasicKafkaIntegration()
    {
        int howManyStoreOffsetsAreCalled = 0;

        var resultsToReturn = new List<ConsumeResult<string, string>>
        {
            new ConsumeResult<string, string> { Topic = "Topic1", Message = new Message<string, string> { Key = "key1", Value = "value1" } },
            new ConsumeResult<string, string> { Topic = "Topic2", Message = new Message<string, string> { Key = "key2", Value = "value2" } },
            new ConsumeResult<string, string> { Topic = "Topic2", Message = new Message<string, string> { Key = "key3", Value = "value3" } },
            new ConsumeResult<string, string> { Topic = "Topic1", Message = new Message<string, string> { Key = "key4", Value = "value4" } }
        };

        MockedConsumerKafka.Setup(x => x.Subscribe(It.Is<IEnumerable<string>>(t => t.Contains("Topic1") || t.Contains("Topic2"))));

        MockedConsumerKafka.Setup(x => x.StoreOffset(It.Is<ConsumeResult<string, string>>(t => resultsToReturn.Contains(t))))
            .Callback(() => howManyStoreOffsetsAreCalled++);

        MockedConsumerKafka.SetupSequence(x => x.Consume(It.IsAny<TimeSpan>()))
            .Returns(resultsToReturn[0])
            .Returns(resultsToReturn[1])
            .Returns((ConsumeResult<string, string>)null!) //add a null here so we ensure this thing keeps going on after the null
            .Returns(resultsToReturn[2])
            .Returns(resultsToReturn[3]);

        var hostedAgentToTest = Provider.GetRequiredService<KafkaConsumerService<string, string>>();
        var processor = (MyUnitTestHostedAgent)Provider.GetRequiredService<IKafkaProcessor<string, string>>();

        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        await hostedAgentToTest.StartAsync(cancellationToken.Token);

        //try to wait until the test passes...Or kill it after 5 seconds
        var spinWaitResult = SpinWait.SpinUntil(() =>
        {
            return howManyStoreOffsetsAreCalled == 4 && processor.MessagesProcessed.Count == 4;

        }, TimeSpan.FromSeconds(10));

        await hostedAgentToTest.StopAsync(cancellationToken.Token);

        //make sure we spun until we found the right amount of records
        Assert.True(spinWaitResult);

        var result = processor.MessagesProcessed;

        Assert.Equal(4, result.Count);
        Assert.Contains(result, x => x.Topic == "Topic1" && x.Message.Key == "key1" && x.Message.Value == "value1");
        Assert.Contains(result, x => x.Topic == "Topic2" && x.Message.Key == "key2" && x.Message.Value == "value2");
        Assert.Contains(result, x => x.Topic == "Topic2" && x.Message.Key == "key3" && x.Message.Value == "value3");
        Assert.Contains(result, x => x.Topic == "Topic1" && x.Message.Key == "key4" && x.Message.Value == "value4");

        MockedConsumerKafka.Verify(x => x.Subscribe(It.Is<IEnumerable<string>>(t => t.Contains("Topic1") || t.Contains("Topic2"))), Times.Once);
        MockedConsumerKafka.Verify(x => x.Consume(It.IsAny<TimeSpan>()), Times.AtLeast(5));
        MockedConsumerKafka.Verify(x => x.StoreOffset(It.Is<ConsumeResult<string, string>>(t => resultsToReturn.Contains(t))), Times.Exactly(4));
    }

    ////background services doesn't raise exceptions based on the github issues. It uses a fire and forget. Don't have a solution other then to log it
    //[Fact]
    //public async Task OnCriticalErrorThrow()
    //{
    //    //force an error
    //    MockedConsumerKafka.Setup(x => x.Subscribe(It.IsAny<IEnumerable<string>>()))
    //        .Throws(new Exception("Some Issue With Kafka"));

    //    await Assert.ThrowsAsync<Exception>(async () =>
    //    {
    //        var hostedAgentToTest = Provider.GetRequiredService<MyUnitTestHostedAgent>();
    //        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15));

    //        var hostedAgentWaitForError = hostedAgentToTest.StartAsync(cancellationToken.Token);

    //        await Task.Delay(5000);

    //        var t = "ASDfasd";
    //        await hostedAgentWaitForError;
    //    });
    //}

}
