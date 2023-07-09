using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace LibraryCore.Kafka;

public class KafkaNodeManager
{
    public KafkaNodeManager()
    {
        RegisteredJobs = new();
    }

    private ConcurrentDictionary<string, RegistrationConfiguration> RegisteredJobs { get; }

    public KafkaNodeManager RegisterJob(string key, int numberOfNodes, Func<IKakfaNode> nodeCreator)
    {
        RegisteredJobs.TryAdd(key, new(nodeCreator, numberOfNodes));
        return this;
    }

    public IEnumerable<Task> CreateNodeAsync(string key, CancellationToken cancellationToken)
    {
        var configuration = RegisteredJobs.Single(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        var tasks = new List<Task>();

        for (int i = 0; i < configuration.Value.NumberOfNodes; i++)
        {
            tasks.Add(configuration.Value.NodeCreator().CreateNodeAsync(i, key, cancellationToken));
        }

        return tasks;
    }

    public record RegistrationConfiguration(Func<IKakfaNode> NodeCreator, int NumberOfNodes);

}

public interface IKakfaNode
{
    Task CreateNodeAsync(int nodeId, string jobKey, CancellationToken cancellationToken);
}

public abstract class KafkaNode<TKafkaKey, TKafkaBody> : IKakfaNode
{
    public KafkaNode(ILogger logger,
                     IEnumerable<string> topicsToRead,
                     IConsumer<TKafkaKey, TKafkaBody> kafkaConsumer)
    {
        Logger = logger;
        TopicsToRead = topicsToRead;
        KafkaConsumer = kafkaConsumer;
    }

    public ILogger Logger { get; }
    public IEnumerable<string> TopicsToRead { get; }
    public IConsumer<TKafkaKey, TKafkaBody> KafkaConsumer { get; }
    public virtual TimeSpan ConsumeTimeout() => TimeSpan.FromSeconds(15);

    private const string LogFormat = "{Action} : NodeId = {NodeId} : JobKey = {JobKey} : AdditionalInfo = {AdditionalInfo}";

    public abstract Task ProcessMessageAsync(ConsumeResult<TKafkaKey, TKafkaBody> messageResult, int nodeId, CancellationToken stoppingToken);
    public virtual void StoreOffset(ConsumeResult<TKafkaKey, TKafkaBody> result) => KafkaConsumer.StoreOffset(result);

    public async Task CreateNodeAsync(int nodeId, string jobKey, CancellationToken cancellationToken)
    {
        var timeout = ConsumeTimeout();

        Logger.LogInformation(LogFormat, "Processor Started", nodeId, jobKey, $"TopicsRead = {string.Join(',', TopicsToRead)}");

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
                    Logger.LogInformation(LogFormat, "Kafka Messaged Received", nodeId, jobKey, $"Key = {consumeResult.Message.Key ?? default}");

                    await ProcessMessageAsync(consumeResult, nodeId, cancellationToken).ConfigureAwait(false);

                    KafkaConsumer.StoreOffset(consumeResult);
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

    private bool LogExceptionAndThrow(Exception ex, int nodeId, string jobKey, CancellationToken cancellationToken, [CallerMemberName] string methodName = "")
    {
        if (cancellationToken.IsCancellationRequested)
        {
            Logger.LogWarning(LogFormat, "Cancellation Token Is Stopping", nodeId, jobKey, string.Empty);
            return false;
        }

        //log any critical errors. Hosted services don't bubble up well with threading like this.
        Logger.LogCritical(ex, LogFormat, "Error In Kafka Hosted Service. Service is not unstable", nodeId, jobKey, string.Empty);

        return true;
    }
}
