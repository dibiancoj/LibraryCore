using Confluent.Kafka;
using System.Collections.Immutable;
using System.Text.Json;
using static LibraryCore.IntegrationTests.Framework.Kafka.Api.KafkaApi;

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

    public static IConsumer<string, PublishModel> BuildConsumerGroup(string consumerGroup)
    {
        return new ConsumerBuilder<string, PublishModel>(new ConsumerConfig(BuildClientConfig())
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

    private static IProducer<string, PublishModel> BuildProducerGroup()
    {
        return new ProducerBuilder<string, PublishModel>(new ProducerConfig(BuildClientConfig()))
        .SetErrorHandler((t, err) =>
        {
            throw new Exception(err.Reason);
        })
        .SetValueSerializer(new KafkaMessagePayloadSerializer())
        .Build();
    }


    public class KafkaMessagePayloadSerializer : IDeserializer<PublishModel>, ISerializer<PublishModel>
    {
        public PublishModel Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonSerializer.Deserialize<PublishModel>(data)!;
        }

        public byte[] Serialize(PublishModel data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
