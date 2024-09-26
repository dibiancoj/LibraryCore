using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Kafka.Settings;

/// <summary>
/// This class is used to hold the settings necessary to connect to a Kafka server.
/// The UserName and UserPassword are optional and only needed if you are connecting to the Kafka Servers
/// </summary>
/// <remarks>This is a partial class which allows you to add extra parameters you need</remarks>
[ExcludeFromCodeCoverage]
public class KafkaAppSettings
{
    /// <summary>
    /// The Kafka server to connect to
    /// </summary>
    [Required]
    public string StreamServers { get; set; } = null!;

    /// <summary>
    /// The username to connect to the Kafka server
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The password to connect to the Kafka server
    /// </summary>
    public string? UserPassword { get; set; }

    /// <summary>
    /// Content of the public cert if needed
    /// </summary>
    public string? RootPublicCertificateContent { get; set; }
}