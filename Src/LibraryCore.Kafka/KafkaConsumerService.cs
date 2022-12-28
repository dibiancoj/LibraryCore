﻿using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace LibraryCore.Kafka;

//TODO: 
//deal with multiple consumers of the same type. Inject IConsumerProvider? Or do it in a fluent way where you specify the configuration with the topics to listen to w/ timeout options, etc.?

//NOTE: If you want multiple readers or you have too much back pressure - use multiple hosted agents which will creat multiple consumer groups. 

//Abstract the kafka consumer into a class. Then inject it here. This way we can run it from whever???

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
    protected virtual TimeSpan KafkaConsumeTimeOut { get; } = new TimeSpan(0, 0, 15);
    protected abstract Task ProcessMessageAsync(ConsumeResult<TKafkaKey, TKafkaMessageBody> messageResult, CancellationToken stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //background services doesn't raise exceptions based on the github issues. It uses a fire and forget. Don't have a solution other then to log it
        var channel = Channel.CreateUnbounded<ConsumeResult<TKafkaKey, TKafkaMessageBody>?>();

        var reader = ReadAndProcessMessageAsync(channel.Reader, stoppingToken);
        var consumeAndPublishChannel = PublishIncomingMessageAsync(channel.Writer, stoppingToken);

        await Task.WhenAll(consumeAndPublishChannel, reader);
    }

    private async Task PublishIncomingMessageAsync(ChannelWriter<ConsumeResult<TKafkaKey, TKafkaMessageBody>?> channelWriter, CancellationToken stoppingToken)
    {
        Logger.LogInformation($"KafkaConsumer.IncomingProcessor:{typeof(TKafkaKey).Name}|{typeof(TKafkaMessageBody)}:Started:ReadingTopics={string.Join(',', TopicsToRead)}");

        //let the other part of the hosted service bootup
        await Task.Delay(100, stoppingToken);

        try
        {
            Consumer.Subscribe(TopicsToRead);

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = Consumer.Consume(KafkaConsumeTimeOut);

                //only publish if it didn't time out and we have an entry from kafka. This is an effort to keep the channel clear
                if (consumeResult != null)
                {
                    //we have a message so go publish. (would be null if it timed out)
                    await channelWriter.WriteAsync(consumeResult, stoppingToken).ConfigureAwait(false);
                }

                //allow threads to get control. We need something that is async to allow threads to continue and run anything needed that is urgent (mainly for time out scenario)
                await Task.Delay(5, stoppingToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            //log any critical errors. Hosted services don't bubble up well with threading like this.
            Logger.LogCritical(ex, $"Error In Kafka Hosted Service. Service is not unstable. Method = {nameof(PublishIncomingMessageAsync)}");
            throw;
        }
    }

    private async Task ReadAndProcessMessageAsync(ChannelReader<ConsumeResult<TKafkaKey, TKafkaMessageBody>?> channelReader, CancellationToken stoppingToken)
    {
        Logger.LogInformation($"KafkaConsumer.Read:{typeof(TKafkaKey).Name}|{typeof(TKafkaMessageBody)}:Started");

        //let the other part of the hosted service bootup
        await Task.Delay(100, stoppingToken).ConfigureAwait(false);

        try
        {
            while (await channelReader.WaitToReadAsync(cancellationToken: stoppingToken).ConfigureAwait(false))
            {
                while (channelReader.TryRead(out var kafkaMessageResult) && kafkaMessageResult != null)
                {
                    Logger.LogInformation($"Kafka Messages Received On {DateTime.Now}");

                    await ProcessMessageAsync(kafkaMessageResult, stoppingToken).ConfigureAwait(false);

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
