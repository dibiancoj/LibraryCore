using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace LibraryCore.Kafka;

//if you need multiple hosted agents running (with the same class) - this way you end up with 2 runners (i reader isn't enough to keep up). This is needed for kafka to save the correct order (multiple consumers).
//see integration test for this example
//builder.Services.AddSingleton<BackgroundService>(x => new MyHostedAgent());
//builder.Services.AddSingleton<BackgroundService>(x => new MyHostedAgent());
public class KafkaConsumerService<TKafkaKey, TKafkaMessageBody> : BackgroundService
{
    public KafkaConsumerService(ILogger<KafkaConsumerService<TKafkaKey, TKafkaMessageBody>> logger, IKafkaProcessor<TKafkaKey, TKafkaMessageBody> kafkaProcessor, int nodeId = 1)
    {
        Logger = logger;
        KafkaProcessor = kafkaProcessor;
        NodeId = nodeId;
        KafkaConsumeTimeOut = kafkaProcessor.KafkaConsumeTimeOut();
    }

    private ILogger<KafkaConsumerService<TKafkaKey, TKafkaMessageBody>> Logger { get; }
    private IKafkaProcessor<TKafkaKey, TKafkaMessageBody> KafkaProcessor { get; }
    public int NodeId { get; }
    private TimeSpan KafkaConsumeTimeOut { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //background services doesn't raise exceptions based on the github issues. It uses a fire and forget. Don't have a solution other then to log it
        var channel = Channel.CreateUnbounded<ConsumeResult<TKafkaKey, TKafkaMessageBody>?>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        });

        var reader = ReadAndProcessMessageAsync(channel.Reader, stoppingToken);
        var consumeAndPublishChannel = PublishIncomingMessageAsync(channel.Writer, stoppingToken);

        await Task.WhenAll(consumeAndPublishChannel, reader);
    }

    private async Task PublishIncomingMessageAsync(ChannelWriter<ConsumeResult<TKafkaKey, TKafkaMessageBody>?> channelWriter, CancellationToken stoppingToken)
    {
        Logger.LogInformation(LogMessage("Started", string.Join(',', KafkaProcessor.TopicsToRead)));

        //let the other part of the hosted service bootup
        await Task.Delay(100, stoppingToken);

        try
        {
            KafkaProcessor.KafkaConsumer.Subscribe(KafkaProcessor.TopicsToRead);

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = KafkaProcessor.KafkaConsumer.Consume(KafkaConsumeTimeOut);

                //only publish if it didn't time out and we have an entry from kafka. This is an effort to keep the channel clear
                if (consumeResult != null)
                {
                    Logger.LogInformation(LogMessage("Kafka Messaged Received", $"Key={consumeResult.Message.Key ?? default}"));

                    //we have a message so go publish. (would be null if it timed out)
                    await channelWriter.WriteAsync(consumeResult, stoppingToken).ConfigureAwait(false);
                }

                //allow threads to get control. We need something that is async to allow threads to continue and run anything needed that is urgent (mainly for time out scenario)
                await Task.Delay(10, stoppingToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            if (LogExceptionAndThrow(ex, stoppingToken))
            {
                throw;
            }
        }
    }

    private async Task ReadAndProcessMessageAsync(ChannelReader<ConsumeResult<TKafkaKey, TKafkaMessageBody>?> channelReader, CancellationToken stoppingToken)
    {
        Logger.LogInformation(LogMessage("Started"));

        //let the other part of the hosted service bootup
        await Task.Delay(100, stoppingToken).ConfigureAwait(false);

        try
        {
            while (await channelReader.WaitToReadAsync(cancellationToken: stoppingToken).ConfigureAwait(false))
            {
                while (channelReader.TryRead(out var kafkaMessageResult) && kafkaMessageResult != null)
                {
                    Logger.LogInformation(LogMessage("Channel Message Received", $"Key={kafkaMessageResult.Message.Key ?? default}"));

                    await KafkaProcessor.ProcessMessageAsync(kafkaMessageResult, NodeId, stoppingToken).ConfigureAwait(false);

                    KafkaProcessor.KafkaConsumer.StoreOffset(kafkaMessageResult);
                }
            }
        }
        catch (Exception ex)
        {
            if (LogExceptionAndThrow(ex, stoppingToken))
            {
                throw;
            }
        }
    }

    private string LogMessage(string command, string? additionalInfo = null, [CallerMemberName] string methodName = "")
    {
        string? additionalInfoOutput = additionalInfo == null ?
                                        null :
                                        $"({additionalInfo})";

        return $"{command} : Node = {NodeId} : Method = {methodName}{additionalInfoOutput} : {DateTime.Now}";
    }

    private bool LogExceptionAndThrow(Exception ex, CancellationToken cancellationToken, [CallerMemberName] string methodName = "")
    {
        if (cancellationToken.IsCancellationRequested)
        {
            Logger.LogWarning(LogMessage("Cancellation Token Is Stopping", methodName: methodName));
            return false;
        }

        //log any critical errors. Hosted services don't bubble up well with threading like this.
        Logger.LogCritical(ex, LogMessage("Error In Kafka Hosted Service. Service is not unstable", methodName: methodName));

        return true;
    }

}
