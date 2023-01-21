using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace LibraryCore.Kafka;

internal class KafkaNodeRunner<TKafkaKey, TKafkaMessageBody>
{
    internal KafkaNodeRunner(int nodeId, ILogger<KafkaConsumerService<TKafkaKey, TKafkaMessageBody>> logger, IKafkaProcessor<TKafkaKey, TKafkaMessageBody> kafkaProcessor)
    {
        NodeId = nodeId;
        Logger = logger;
        KafkaProcessor = kafkaProcessor;
    }

    private int NodeId { get; }
    private ILogger<KafkaConsumerService<TKafkaKey, TKafkaMessageBody>> Logger { get; }
    private IKafkaProcessor<TKafkaKey, TKafkaMessageBody> KafkaProcessor { get; }

    internal async Task CreateNodeAsync(CancellationToken cancellationToken)
    {
        await ProcessorAsync(cancellationToken);
    }

    private async Task ProcessorAsync(CancellationToken stoppingToken)
    {
        var timeout = KafkaProcessor.KafkaConsumeTimeOut();

        Logger.LogInformation(LogMessage("Started", string.Join(',', KafkaProcessor.TopicsToRead)));

        //let the other part of the hosted service bootup
        await Task.Delay(100, stoppingToken).ConfigureAwait(false);

        try
        {
            KafkaProcessor.KafkaConsumer.Subscribe(KafkaProcessor.TopicsToRead);

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = KafkaProcessor.KafkaConsumer.Consume(timeout);

                //only publish if it didn't time out and we have an entry from kafka. This is an effort to keep the channel clear
                if (consumeResult != null)
                {
                    Logger.LogInformation(LogMessage("Kafka Messaged Received", $"Key={consumeResult.Message.Key ?? default}"));

                    await KafkaProcessor.ProcessMessageAsync(consumeResult, NodeId, stoppingToken).ConfigureAwait(false);

                    KafkaProcessor.KafkaConsumer.StoreOffset(consumeResult);
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
