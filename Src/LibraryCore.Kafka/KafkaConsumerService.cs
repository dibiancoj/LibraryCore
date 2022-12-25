using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace LibraryCore.Kafka;

//TODO: 
//deal with multiple consumers of the same type. Inject IConsumerProvider? Or do it in a fluent way where you specify the configuration with the topics to listen to w/ timeout options, etc.?

public abstract class KafkaConsumerService<TKafkaKey, TKafkaMessageBody> : BackgroundService
{
    public KafkaConsumerService(ILogger<KafkaConsumerService<TKafkaKey, TKafkaMessageBody>> logger, IConsumer<TKafkaKey, TKafkaMessageBody> consumer)
    {
        Logger = logger;
        Consumer = consumer;
    }

    private ILogger<KafkaConsumerService<TKafkaKey, TKafkaMessageBody>> Logger { get; }
    private IConsumer<TKafkaKey, TKafkaMessageBody> Consumer { get; }

    protected abstract IEnumerable<string> TopicsToRead { get; }
    protected abstract int NumberOfReaders { get; }
    protected virtual TimeSpan KafkaConsumeTimeOut { get; } = new TimeSpan(0, 0, 15);
    protected abstract Task ProcessMessageAsync(ConsumeResult<TKafkaKey, TKafkaMessageBody> messageResult, int NodeIndex, CancellationToken stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = Channel.CreateUnbounded<ConsumeResult<TKafkaKey, TKafkaMessageBody>?>();

        var readers = new List<Task>();

        for (int i = 0; i < NumberOfReaders; i++)
        {
            readers.Add(ReadAndProcessMessageAsync(channel.Reader, i, stoppingToken));
        }

        var consumeAndPublishChannel = PublishIncomingMessageAsync(channel.Writer, stoppingToken);

        await Task.WhenAll(readers.Append(consumeAndPublishChannel));
    }

    private async Task PublishIncomingMessageAsync(ChannelWriter<ConsumeResult<TKafkaKey, TKafkaMessageBody>?> channelWriter, CancellationToken stoppingToken)
    {
        Logger.LogInformation($"KafkaConsumer.IncomingProcessor:{typeof(TKafkaKey).Name}|{typeof(TKafkaMessageBody)}:Started:ReadingTopics={string.Join(',', TopicsToRead)}");

        //let the other part of the hosted service bootup
        await Task.Delay(500, stoppingToken);

        try
        {
            Consumer.Subscribe(TopicsToRead);

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = Consumer.Consume(KafkaConsumeTimeOut);

                //only publish if it didn't time out and we have an entry from kafka. This is an effort to keep the channel clear
                if (consumeResult != null)
                {
                    await channelWriter.WriteAsync(consumeResult, stoppingToken).ConfigureAwait(false);
                }
                else
                {
                    //if we have a timeout...wait a bit to let the other tasks carry on
                    await Task.Delay(50, stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            //log any critical errors. Hosted services don't bubble up well with threading like this.
            Logger.LogCritical(ex, $"Error In Kafka Hosted Service. Service is not unstable. Method = {nameof(PublishIncomingMessageAsync)}");
            throw;
        }
    }

    private async Task ReadAndProcessMessageAsync(ChannelReader<ConsumeResult<TKafkaKey, TKafkaMessageBody>?> channelReader, int nodeIndex, CancellationToken stoppingToken)
    {
        Logger.LogInformation($"KafkaConsumer.Read:{typeof(TKafkaKey).Name}|{typeof(TKafkaMessageBody)}:Node={nodeIndex}:Started");

        //let the other part of the hosted service bootup
        await Task.Delay(500, stoppingToken);

        try
        {
            while (await channelReader.WaitToReadAsync(cancellationToken: stoppingToken).ConfigureAwait(false))
            {
                while (channelReader.TryRead(out var kafkaMessageResult) && kafkaMessageResult != null)
                {
                    Logger.LogInformation($"Kafka Messages Received At Node = {nodeIndex} On {DateTime.Now}");

                    await ProcessMessageAsync(kafkaMessageResult, nodeIndex, stoppingToken);

                    Consumer.StoreOffset(kafkaMessageResult);
                }
            }
        }
        catch (Exception ex)
        {
            //log any critical errors. Hosted services don't bubble up well with threading like this.
            Logger.LogCritical(ex, $"Error In Kafka Hosted Service. Service is not unstable. Method = {nameof(ReadAndProcessMessageAsync)}");
            throw;
        }
    }

}
