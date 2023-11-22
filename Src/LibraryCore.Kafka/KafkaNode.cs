using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace LibraryCore.Kafka;

public abstract class KafkaNode<TKafkaKey, TKafkaBody>(ILogger logger,
                                                       IEnumerable<string> topicsToRead,
                                                       IConsumer<TKafkaKey, TKafkaBody> kafkaConsumer) : IKafkaNodeCreator
{
    public ILogger Logger { get; } = logger;
    public IEnumerable<string> TopicsToRead { get; } = topicsToRead;
    public IConsumer<TKafkaKey, TKafkaBody> KafkaConsumer { get; } = kafkaConsumer;
    public virtual TimeSpan ConsumeTimeout() => TimeSpan.FromSeconds(15);

    private const string LogFormat = "{Action} : NodeId = {NodeId} : JobKey = {JobKey}";
    private const string LogFormatOnStartup = LogFormat + " : TopicsRead = {TopicsRead}";
    private const string LogFormatOnMessageReceived = LogFormat + " : MessageReadKey = {MessageKey}";

    public abstract Task ProcessMessageAsync(ConsumeResult<TKafkaKey, TKafkaBody> messageResult, int nodeId, CancellationToken stoppingToken);
    public virtual void StoreOffsetByConsumer(ConsumeResult<TKafkaKey, TKafkaBody> result) => KafkaConsumer.StoreOffset(result);

    public async Task CreateNodeAsync(int nodeId, string jobKey, CancellationToken cancellationToken)
    {
        var timeout = ConsumeTimeout();

        Logger.LogInformation(LogFormatOnStartup, "Processor Started", nodeId, jobKey, string.Join(',', TopicsToRead));

        //let the other part of the hosted service bootup
        await Task.Delay(100, cancellationToken).ConfigureAwait(false);

        try
        {
            KafkaConsumer.Subscribe(TopicsToRead);

            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = KafkaConsumer.Consume(timeout);

                //only publish if it didn't time out and we have an entry from kafka. This is an effort to keep the channel clear
                if (consumeResult != null)
                {
                    Logger.LogInformation(LogFormatOnMessageReceived, "Kafka Messaged Received", nodeId, jobKey, consumeResult.Message.Key ?? default);

                    await ProcessMessageAsync(consumeResult, nodeId, cancellationToken).ConfigureAwait(false);

                    //allow the consumer to control the offset after processing is complete. This way if they want to manually consume it, etc.
                    StoreOffsetByConsumer(consumeResult);
                }

                //allow threads to get control. We need something that is async to allow threads to continue and run anything needed that is urgent (mainly for time out scenario)
                await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            if (LogExceptionAndThrow(ex, nodeId, jobKey, cancellationToken))
            {
                throw;
            }
        }
    }

    private bool LogExceptionAndThrow(Exception ex, int nodeId, string jobKey, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            Logger.LogWarning(LogFormat, "Cancellation Token Is Stopping", nodeId, jobKey);
            return false;
        }

        //log any critical errors. Hosted services don't bubble up well with threading like this.
        Logger.LogCritical(ex, LogFormat, "Error In Kafka Hosted Service. Service is not unstable", nodeId, jobKey);

        return true;
    }
}

