using Confluent.Kafka;
using LibraryCore.Kafka.Models;
using LibraryCore.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LibraryCore.Tests.Kafka.HostedServices;

public class ConsumerServiceTest
{
    [Fact]
    public async Task BasicSuccessfulTest()
    {
        var testDataSet = Enumerable.Range(0, 2).Select(x => new MyMessage(Guid.NewGuid(), $"Test{x}")).ToArray();
        var mockDataStore = new ConcurrentQueue<ConsumeResult<string, KafkaNullableOfT<MyMessage>>>(testDataSet.Select(t => CreateMessage(t)).ToArray());

        var mockConsumer = new Mock<IConsumer<string, KafkaNullableOfT<MyMessage>>>();

        var myHostedService = new Mock<MyHostedService>(new Mock<ILogger<MyHostedService>>().Object, Options.Create(new KafkaAppSettings()), 1)
        {
            CallBase = true
        };

        myHostedService.Setup(x => x.BuildConsumer("MyConsumerGroup", AutoOffsetReset.Earliest, false, true))
            .Returns(mockConsumer.Object);

        mockConsumer.Setup(x => x.Consume(It.IsAny<TimeSpan>()))
            .Returns(() =>
            {
                if (mockDataStore.TryDequeue(out var result))
                {
                    return result;
                }

                return null!;
            });

        mockConsumer.Setup(x => x.StoreOffset(It.IsAny<ConsumeResult<string, KafkaNullableOfT<MyMessage>>>()));

        var tsk = myHostedService.Object.StartAsync(CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await myHostedService.Object.StopAsync(CancellationToken.None);

        Assert.Equal(2, myHostedService.Object.MyDatabase.Count);
        Assert.True(myHostedService.Object.MyDatabase.Values.All(t => t.IsCommitted));
        Assert.Contains(myHostedService.Object.MyDatabase.Values, x => x.Message.Description == "Test0");
        Assert.Contains(myHostedService.Object.MyDatabase.Values, x => x.Message.Description == "Test1");

        myHostedService.VerifyAll();
        mockConsumer.Verify(x => x.Consume(It.IsAny<TimeSpan>()));
        mockConsumer.Verify(x => x.StoreOffset(It.IsAny<ConsumeResult<string, KafkaNullableOfT<MyMessage>>>()), Times.Exactly(testDataSet.Length));
    }

    [Fact]
    public async Task HighLoadThreadTest()
    {
        var testDataSet = Enumerable.Range(0, 50).Select(x => new MyMessage(Guid.NewGuid(), $"Test{x}")).ToArray();
        var mockDataStore = new ConcurrentQueue<ConsumeResult<string, KafkaNullableOfT<MyMessage>>>(testDataSet.Select(t => CreateMessage(t)).ToArray());

        var mockConsumer = new Mock<IConsumer<string, KafkaNullableOfT<MyMessage>>>();

        var myHostedService = new Mock<MyHostedService>(new Mock<ILogger<MyHostedService>>().Object, Options.Create(new KafkaAppSettings()), 5)
        {
            CallBase = true
        };

        myHostedService.Setup(x => x.BuildConsumer("MyConsumerGroup", AutoOffsetReset.Earliest, false, true))
            .Returns(mockConsumer.Object);

        mockConsumer.Setup(x => x.Consume(It.IsAny<TimeSpan>()))
            .Returns(() =>
            {
                if (mockDataStore.TryDequeue(out var result))
                {
                    return result;
                }

                return null!;
            });

        var tsk = myHostedService.Object.StartAsync(CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        await myHostedService.Object.StopAsync(CancellationToken.None);

        Assert.Equal(50, myHostedService.Object.MyDatabase.Count);
        Assert.True(myHostedService.Object.MyDatabase.Values.All(t => t.IsCommitted));
        Assert.All(testDataSet, x => myHostedService.Object.MyDatabase.ContainsKey(x.Id));

        myHostedService.VerifyAll();
        mockConsumer.Verify(x => x.Consume(It.IsAny<TimeSpan>()));
        mockConsumer.Verify(x => x.StoreOffset(It.IsAny<ConsumeResult<string, KafkaNullableOfT<MyMessage>>>()), Times.Exactly(testDataSet.Length));
    }

    private static ConsumeResult<string, KafkaNullableOfT<MyMessage>> CreateMessage(MyMessage myMessage)
    {
        return new ConsumeResult<string, KafkaNullableOfT<MyMessage>>
        {
            Message = new Message<string, KafkaNullableOfT<MyMessage>>
            {
                Value = KafkaNullableOfT<MyMessage>.SuccessfulMessageSchema(myMessage)
            }
        };
    }
}