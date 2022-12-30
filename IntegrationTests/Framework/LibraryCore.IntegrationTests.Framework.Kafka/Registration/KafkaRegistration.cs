using Confluent.Kafka;
using LibraryCore.IntegrationTests.Framework.Kafka.Api.Models;
using System.Collections.Immutable;
using System.Text.Json;

namespace LibraryCore.IntegrationTests.Framework.Kafka.Registration;

public static class KafkaRegistration
{
    private const string bootstrapServer = "localhost:9093";
    public static IImmutableList<string> TopicsToUse { get; } = new List<string> { "topic-123" }.ToImmutableList();

    public static void RegisterKakfa(this WebApplicationBuilder builder)
    {
        //kafka admin client
        builder.Services.AddSingleton(sp => new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = bootstrapServer
        }).Build());

        //kafka producer
        builder.Services.AddSingleton(sp => BuildProducerGroup());
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

    public static IConsumer<string, KafkaMessageModel> BuildConsumerGroup(string consumerGroup)
    {
        return new ConsumerBuilder<string, KafkaMessageModel>(new ConsumerConfig(BuildClientConfig())
        {
            GroupId = consumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoOffsetStore = false,
            EnableAutoCommit = true
            //AllowAutoCreateTopics = true not working KafkaMessagePayloadSerializer latest version based on threads (docker compose is set)
        })
        .SetValueDeserializer(new KafkaMessagePayloadSerializer())
        .SetErrorHandler((t, err) =>
        {
            throw new Exception(err.Reason);
        }).Build();
    }

    private static IProducer<string, KafkaMessageModel> BuildProducerGroup()
    {
        return new ProducerBuilder<string, KafkaMessageModel>(new ProducerConfig(BuildClientConfig()))
        .SetErrorHandler((t, err) =>
        {
            throw new Exception(err.Reason);
        })
        .SetValueSerializer(new KafkaMessagePayloadSerializer())
        .Build();
    }

    public class KafkaMessagePayloadSerializer : IDeserializer<KafkaMessageModel>, ISerializer<KafkaMessageModel>
    {
        public KafkaMessageModel Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonSerializer.Deserialize<KafkaMessageModel>(data) ?? throw new Exception("Can't Deserialize The Kafka Message To PublishModel");
        }

        public byte[] Serialize(KafkaMessageModel data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
