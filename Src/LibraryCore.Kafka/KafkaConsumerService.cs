﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LibraryCore.Kafka;

//if you need multiple hosted agents running (with the same class) - this way you end up with 2 runners (i reader isn't enough to keep up). This is needed for kafka to save the correct order (multiple consumers).
//see integration test for this example
//builder.Services.AddSingleton<BackgroundService>(x => new MyHostedAgent());
//builder.Services.AddSingleton<BackgroundService>(x => new MyHostedAgent());
public class KafkaConsumerService<TKafkaKey, TKafkaMessageBody> : BackgroundService
{
    public KafkaConsumerService(ILogger<KafkaConsumerService<TKafkaKey, TKafkaMessageBody>> logger, IKafkaProcessor<TKafkaKey, TKafkaMessageBody> kafkaProcessor, int numberOfNodes = 1)
    {
        Logger = logger;
        KafkaProcessor = kafkaProcessor;
        NumberOfNodes = numberOfNodes;
    }

    private ILogger<KafkaConsumerService<TKafkaKey, TKafkaMessageBody>> Logger { get; }
    private IKafkaProcessor<TKafkaKey, TKafkaMessageBody> KafkaProcessor { get; }
    public int NumberOfNodes { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < NumberOfNodes; i++)
        {
            tasks.Add(new KafkaThreadRunner<TKafkaKey, TKafkaMessageBody>(i + 1, Logger, KafkaProcessor).CreateThreadAsync(stoppingToken));
        }

        await Task.WhenAll(tasks);
    }
}
