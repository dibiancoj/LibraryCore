using LibraryCore.Kafka.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Kafka.Registration;

[ExcludeFromCodeCoverage]
public static class KafkaRegistration
{
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode(Shared.ErrorMessages.AotDynamicAccess)]
#endif
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(Shared.ErrorMessages.AotDynamicAccess)]
#endif
    public static IServiceCollection AddKafkaConsumer<TKafkaMessage, TKafkaAppSettings>(this IServiceCollection services, IConfiguration configuration)
        where TKafkaAppSettings : KafkaAppSettings
    {
        return services.AddKafkaConsumer<TKafkaMessage, TKafkaAppSettings>(configuration.GetSection("KafkaSettings"));
    }

#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode(Shared.ErrorMessages.AotDynamicAccess)]
#endif
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(Shared.ErrorMessages.AotDynamicAccess)]
#endif
    public static IServiceCollection AddKafkaConsumer<TKafkaMessage, TKafkaAppSettings>(this IServiceCollection services, IConfigurationSection configurationSectionWithSettings)
     where TKafkaAppSettings : KafkaAppSettings
    {
        services.AddOptions<TKafkaAppSettings>().Bind(configurationSectionWithSettings).ValidateOnStart();

        return services;
    }
}
