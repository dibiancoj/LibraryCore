using LibraryCore.Kafka;

namespace LibraryCore.IntegrationTests.Framework.Kafka.KafkaProcessor;

public class MyHostedAgent(KafkaNodeManager kafkaNodeManager) : BackgroundService
{
    private KafkaNodeManager KafkaNodeManager { get; } = kafkaNodeManager;
    public const string KakfaJobName = "Job1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(KafkaNodeManager.CreateNodeAsync(KakfaJobName, stoppingToken));
    }
}