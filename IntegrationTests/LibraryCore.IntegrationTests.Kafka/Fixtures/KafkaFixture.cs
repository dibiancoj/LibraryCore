
using Castle.Core.Logging;
using Confluent.Kafka;
using LibraryCore.Kafka;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibraryCore.IntegrationTests.Kafka.Fixtures;

public class KafkaFixture
{

#if DEBUG
    public const string SkipReason = "";// Don't want to run kafka integration test locally for now.";
#else
    public const string SkipReason = "";
#endif

    private static bool RunUnitTest() => string.IsNullOrEmpty(SkipReason);

    public IServiceProvider Provider { get; } = null!;
    public const string TopicToTestWith = "topic-123";

    public KafkaFixture()
    {
        if (!RunUnitTest())
        {
            return;
        }

        const string bootstrapServer = "localhost:9093";

        var clientConfig = new ClientConfig
        {
            BootstrapServers = bootstrapServer,
            SaslMechanism = SaslMechanism.Plain, //SaslMechanism.ScramSha512,
            SaslUsername = null,//kafkaSettings.Value.UserName,
            SaslPassword = null,//kafkaSettings.Value.UserPassword,
            SecurityProtocol = SecurityProtocol.Plaintext//string.IsNullOrEmpty(kafkaSettings.Value.UserPassword) ? SecurityProtocol.Plaintext : SecurityProtocol.SaslSsl //only set auth if we have a user name and password
        };

        Provider = new ServiceCollection()
        .AddLogging()
          .AddSingleton(sp => new ConsumerBuilder<string, string>(new ConsumerConfig(clientConfig)
          {
              GroupId = Guid.NewGuid().ToString(),//doing this so we don't cause an inbalance and make it take alot longer and possibly timeou
              AutoOffsetReset = AutoOffsetReset.Earliest,
              EnableAutoOffsetStore = false,
              EnableAutoCommit = true,
              //AllowAutoCreateTopics = true not working in latest version based on threads (docker compose is set)
          }).SetErrorHandler((t, err) =>
          {
              throw new Exception(err.Reason);
          }).Build())
          .AddSingleton(sp => new ProducerBuilder<string, string>(new ProducerConfig(clientConfig))
          .SetErrorHandler((t, err) =>
          {
              throw new Exception(err.Reason);
          }).Build())
          .AddSingleton(sp => new AdminClientBuilder(new AdminClientConfig
          {
              BootstrapServers = bootstrapServer
          }).Build())
          .AddScoped<MyIntegrationHostedAgent>()
          .BuildServiceProvider();
    }
}
