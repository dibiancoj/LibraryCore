using LibraryCore.Kafka;

namespace LibraryCore.IntegrationTests.Framework.Kafka.KafkaProcessor;

public class MyHostedAgent : BackgroundService
{
    public MyHostedAgent(KafkaNodeManager kafkaNodeManager)
    {
        KafkaNodeManager = kafkaNodeManager;
    }

    private KafkaNodeManager KafkaNodeManager { get; }
    public const string KakfaJobName = "Job1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(KafkaNodeManager.CreateNodeAsync(KakfaJobName, stoppingToken));
    }
}