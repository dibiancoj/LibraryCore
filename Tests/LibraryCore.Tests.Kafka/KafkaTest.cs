using Confluent.Kafka;
using LibraryCore.Tests.Kafka.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryCore.Tests.Kafka;

public class KafkaTest
{
    [Fact]
    public async Task BasicKafkaIntegration()
    {
        var mockedKafkaConsumer = new Mock<IConsumer<string, string>>();

        mockedKafkaConsumer.Setup(x => x.Subscribe(It.Is<IEnumerable<string>>(t => t.Contains("Topic1"))));

        mockedKafkaConsumer.SetupSequence(x => x.Consume(It.IsAny<TimeSpan>()))
            .Returns(new ConsumeResult<string, string> { Message = new Message<string, string> { Key = "key1", Value = "value1" } })
            .Returns(new ConsumeResult<string, string> { Message = new Message<string, string> { Key = "key2", Value = "value2" } })
            .Returns((ConsumeResult<string, string>)null!) //add a null here so we ensure this thing keeps going on after the null
            .Returns(new ConsumeResult<string, string> { Message = new Message<string, string> { Key = "key3", Value = "value3" } });

        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(mockedKafkaConsumer.Object)
            .AddSingleton<MyUnitTestHostedAgent>()
            .BuildServiceProvider();

        var hostedAgentToTest = serviceProvider.GetRequiredService<MyUnitTestHostedAgent>();

        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        await hostedAgentToTest.StartAsync(cancellationToken.Token);

        await Task.Delay(3000);

        await hostedAgentToTest.StopAsync(cancellationToken.Token);

        var result = hostedAgentToTest.MessagesProcessed;

        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.Key == "key1" && x.Value.Message == "value1");
        Assert.Contains(result, x => x.Key == "key2" && x.Value.Message == "value2");
        Assert.Contains(result, x => x.Key == "key3" && x.Value.Message == "value3");

        mockedKafkaConsumer.Verify(x => x.Subscribe(It.Is<IEnumerable<string>>(t => t.Contains("Topic1"))), Times.Once);
        mockedKafkaConsumer.Verify(x => x.Consume(It.IsAny<TimeSpan>()), Times.AtLeast(4));
    }

}
