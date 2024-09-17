using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Services;
using System.Collections.Immutable;
using System.Text.Json;

namespace LibraryCore.IntegrationTests.Framework.Kafka.Registration;

public static class KafkaRegistration
{
    private const string bootstrapServer = "localhost:9093";
    public static IImmutableList<string> TopicsToUse { get; } = new List<string> { "my-topic-1" }.ToImmutableList();

    public static void RegisterKakfa(this WebApplicationBuilder builder)
    {
        //kafka admin client
        _ = builder.Services.AddSingleton(sp => new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = bootstrapServer
        }).Build());

        //kafka producer
        _ = builder.Services.AddSingleton(sp => BuildProducerGroup());
    }

    private static ClientConfig BuildClientConfig()
    {
        return new ClientConfig
        {
            BootstrapServers = bootstrapServer,
            SaslMechanism = SaslMechanism.Plain, //SaslMechanism.ScramSha512,
            SaslUsername = null,//kafkaSettings.Value.UserName,
            SaslPassword = null,//kafkaSettings.Value.UserPassword,
            SecurityProtocol = SecurityProtocol.Plaintext//string.IsNullOrEmpty(kafkaSettings.Value.UserPassword) ? SecurityProtocol.Plaintext : SecurityProtocol.SaslSsl //only set auth if we have a user name and password
        };
    }

    private static IProducer<string, KafkaTopic1MessagePayload> BuildProducerGroup()
    {
        return new ProducerBuilder<string, KafkaTopic1MessagePayload>(new ProducerConfig(BuildClientConfig()))
        .SetErrorHandler((t, err) =>
        {
            throw new Exception(err.Reason);
        })
        .SetValueSerializer(new KafkaMessagePayloadSerializer())
        .Build();
    }

    public class KafkaMessagePayloadSerializer : IDeserializer<KafkaTopic1MessagePayload>, ISerializer<KafkaTopic1MessagePayload>
    {
        public KafkaTopic1MessagePayload Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonSerializer.Deserialize<KafkaTopic1MessagePayload>(data) ?? throw new Exception("Can't Deserialize The Kafka Message To PublishModel");
        }

        public byte[] Serialize(KafkaTopic1MessagePayload data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
