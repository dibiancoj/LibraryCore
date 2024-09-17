using Confluent.Kafka;

namespace LibraryCore.Kafka.Settings;

/// <summary>
/// Common code to create the client config for kafka
/// </summary>
public static class ClientConfigurationBuilder
{
    /// <summary>
    /// This method is used to create the client configuration for Kafka
    /// </summary>
    /// <param name="kafkaAppSettings">Kakfa app settings</param>
    /// <returns>ClientConfig</returns>
    public static ClientConfig KafkaConfiguration(KafkaAppSettings kafkaAppSettings)
    {
        return new ClientConfig
        {
            BootstrapServers = kafkaAppSettings.StreamServers,
            SaslMechanism = SaslMechanism.Plain,
            SecurityProtocol = string.IsNullOrEmpty(kafkaAppSettings.UserPassword) ? SecurityProtocol.Plaintext : SecurityProtocol.SaslSsl,
            SaslUsername = kafkaAppSettings.UserName,
            SaslPassword = kafkaAppSettings.UserPassword,
            SslCaPem = kafkaAppSettings.RootPublicCertificateContent
        };
    }
}
