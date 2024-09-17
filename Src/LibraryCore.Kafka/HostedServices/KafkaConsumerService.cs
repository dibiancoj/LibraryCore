using Confluent.Kafka;
using LibraryCore.Kafka.Models;
using LibraryCore.Kafka.Serialization;
using LibraryCore.Kafka.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LibraryCore.Kafka.HostedServices;

/// <summary>
/// Abstract hosted service which allows you to run a kafka consumer and process messages.
/// </summary>
/// <typeparam name="T">Type of the kafka message</typeparam>
/// <param name="logger">Logger</param>
/// <param name="kafkaAppSettings">app settings for kafka</param>
public abstract class KafkaConsumerService<T>(ILogger<KafkaConsumerService<T>> logger, IOptions<KafkaAppSettings> kafkaAppSettings)
    : BackgroundService

{
    private ConcurrentBag<Task> RunningNodes { get; } = [];

    /// <summary>
    /// Minimum nodes you want to run to look for and process messages. Currently, the threads is a static value. Future functionailty might include dynamic scaling.
    /// The default value is 1 if you don't override this property
    /// </summary>
    public virtual int MinimumNumberOfNodes => 1;

    /// <summary>
    /// Topic to consume from for this service
    /// </summary>
    public abstract string TopicToConsumeFrom { get; }

    /// <summary>
    /// Consumer group name to use
    /// </summary>
    public abstract string ConsumerGroup { get; }

    /// <summary>
    /// If there are no messages consumed, we will back off for this time period. The default is 15 seconds.
    /// </summary>
    public virtual TimeSpan NoMessageBackOffPeriod => TimeSpan.FromSeconds(15);

    /// <summary>
    /// Which offset to use. Earliest is the default value
    /// </summary>
    public virtual AutoOffsetReset OffsetReset { get; } = AutoOffsetReset.Earliest;

    protected ILogger<KafkaConsumerService<T>> Logger { get; } = logger;
    protected IOptions<KafkaAppSettings> KafkaAppSettings { get; } = kafkaAppSettings;

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //allow the rest of the api to bootup
        await Task.Delay(100, cancellationToken: stoppingToken);

        //go create the nodes to run
        for (int i = 0; i < MinimumNumberOfNodes; i++)
        {
            RunningNodes.Add(SubscribeAndListenToMessages(i + 1, stoppingToken));
        }

        await Task.WhenAll(RunningNodes);
    }

    private async Task SubscribeAndListenToMessages(int nodeId, CancellationToken stoppingToken)
    {
        Logger.LogInformation("Kafka.SubscribeAndListenToMessages:Started:KakfaMessageType={KakfaMessageType}:NodeId={nodeId}", typeof(T).Name, nodeId);

        //let the rest of the threads start up
        await Task.Delay(100, stoppingToken);

        try
        {
            using var kafkaConsumer = BuildConsumer(ConsumerGroup, OffsetReset, false, true);

            kafkaConsumer.Subscribe(TopicToConsumeFrom);

            while (!stoppingToken.IsCancellationRequested)
            {
                //allow the auto commit to happen.
                await Task.Delay(100, stoppingToken);

                var consumeResult = kafkaConsumer.Consume(TimeSpan.FromSeconds(15));

                //if we don't have a message (if timeout...this will be null)...back off for a bit
                if (consumeResult == null)
                {
                    Logger.LogTrace("No Message Received. Backing Off. KakfaMessageType = {KakfaMessageType}. Node = {NodeId}", typeof(T).Name, nodeId);
                    await Task.Delay(NoMessageBackOffPeriod, stoppingToken);
                    continue;
                }

                Logger.LogTrace("Message Received. Pre-Process Message. KakfaMessageType = {KakfaMessageType}. Node = {NodeId}", typeof(T).Name, nodeId);

                //we have a message - go process it
                await ProcessMessageAsync(kafkaConsumer, consumeResult, nodeId);
                await MarkMessageAsProcessedAsync(kafkaConsumer, consumeResult, nodeId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error In Kafka Hosted Service. KakfaMessageType = {KakfaMessageType}", typeof(T).Name);
        }
    }

    /// <summary>
    /// Method to implement to process the message.
    /// </summary>
    /// <param name="consumer">consumer object incase its needed</param>
    /// <param name="consumeResult">The messge received</param>
    /// <param name="node">node that is processing this</param>
    /// <returns>Completed task</returns>
    protected abstract Task ProcessMessageAsync(IConsumer<string, KafkaNullableOfT<T>> consumer, ConsumeResult<string, KafkaNullableOfT<T>> consumeResult, int node);

    /// <summary>
    /// The method to commit the message if needed. Otherwise, it will just do a basic commit.
    /// </summary>
    /// <param name="consumer">consumer object incase its needed</param>
    /// <param name="consumeResult">The messge received</param>
    /// <param name="node">node that is processing this</param>
    /// <returns>Completed task</returns>
    protected virtual ValueTask MarkMessageAsProcessedAsync(IConsumer<string, KafkaNullableOfT<T>> consumer, ConsumeResult<string, KafkaNullableOfT<T>> consumeResult, int node)
    {
        consumer.StoreOffset(consumeResult);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Control the way a consumer is created. The default is to create a consumer with the default settings. 
    /// If you override the enable auto offset store, you will need to handle the offset commits yourself in the CommitMessageAsync overridable method.
    /// </summary>
    /// <param name="consumerGroup">Consumer group</param>
    /// <param name="offsetReset">Offset reset</param>
    /// <param name="enableAutoOffsetStore">Enable auto offset store</param>
    /// <param name="enableAutoCommit">Enable auto commit</param>
    /// <returns>The consumer which is used in the hosted service</returns>
    /// <exception cref="Exception">General exception when the builder throws</exception>
    public virtual IConsumer<string, KafkaNullableOfT<T>> BuildConsumer(string consumerGroup, AutoOffsetReset offsetReset, bool enableAutoOffsetStore, bool enableAutoCommit)
    {
        return new ConsumerBuilder<string, KafkaNullableOfT<T>>(new ConsumerConfig(ClientConfigurationBuilder.KafkaConfiguration(KafkaAppSettings.Value))
        {
            GroupId = consumerGroup,
            AutoOffsetReset = offsetReset,
            EnableAutoOffsetStore = enableAutoOffsetStore,
            EnableAutoCommit = enableAutoCommit
        })
        .SetValueDeserializer(new NullableKafkaMessageSerializer<T>()) //at some point we will need to allow the consumer to set this. For now will leave it
        .SetErrorHandler((_, e) =>
        {
            Logger.LogCritical("Build Kafka Consumer Failed = {Reason}", e.Reason);
            throw new Exception("Build Kafka Consumer Failed = " + e.Reason);
        })
        .Build();
    }
}