using LibraryCore.Kafka.Settings;
using System.ComponentModel.DataAnnotations;

namespace LibraryCore.IntegrationTests.Framework.Kafka.Settings;

public class SampleKafkaSettings : KafkaAppSettings
{
    [Required]
    public string Topic { get; set; } = null!;

    [Required]
    public string ConsumerGroup { get; set; } = null!;

    [Required]
    public int MinimumNumberOfNodes { get; set; }
}
