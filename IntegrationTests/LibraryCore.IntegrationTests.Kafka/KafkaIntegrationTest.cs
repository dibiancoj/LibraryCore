using Confluent.Kafka;
using LibraryCore.IntegrationTests.Kafka.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryCore.IntegrationTests.Kafka;

public class KafkaIntegrationTest
{
    [Fact(Skip = KafkaFixture.SkipReason)]
    public async Task FullIntegrationTest()
    {
        ////TODO:
        ////1. need to replace with a real consumer
        ////2. Put this in the web api project as a real hosted service

        //var mockedKafkaConsumer = new Mock<IConsumer<string, string>>();

        //mockedKafkaConsumer.Setup(x => x.Consume(It.IsAny<TimeSpan>()))
        //    .Returns(new ConsumeResult<string, string> { Message = new Message<string, string> { Key = "key1", Value = "value1" } });

        //var serviceProvider = new ServiceCollection()
        //    .AddLogging()
        //    .AddSingleton(mockedKafkaConsumer.Object)
        //    .AddSingleton<MyIntegrationHostedAgent>()
        //    .BuildServiceProvider();

        //var hostedAgentToTest = serviceProvider.GetRequiredService<MyIntegrationHostedAgent>();

        //var cancellationToken = new CancellationTokenSource();

        //await hostedAgentToTest.StartAsync(cancellationToken.Token);

        //await Task.Delay(5000);

        //await hostedAgentToTest.StopAsync(cancellationToken.Token);

        //var result = hostedAgentToTest.MessagesProcessed;

        //Assert.Equal(2, result.Count);
    }
}
