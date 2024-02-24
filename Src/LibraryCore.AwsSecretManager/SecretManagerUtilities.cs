using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace LibraryCore.AwsSecretManager;

public static class SecretManagerUtilities
{
    //AmazonSecretsManagerClient is the IAmazonSecretsManager implementation.
    //In DI services.AddAWSService<IAmazonSecretsManager>();
    public static async Task<string?> GetSecretAsync(IAmazonSecretsManager client, string secretArnOrName, string versionStage = "AWSCURRENT", CancellationToken cancellationToken = default)
    {
        var tempResponse = await client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = secretArnOrName,
            VersionStage = versionStage
        }, cancellationToken).ConfigureAwait(false);

        if (tempResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception("Get Secret Value Async Resulted In " + tempResponse.HttpStatusCode);
        }

        return tempResponse switch
        {
            { SecretString: { } } => tempResponse.SecretString, //is not null pattern matching
            { SecretBinary: { } } => StreamToString(tempResponse.SecretBinary), //is not null pattern matching
            _ => null //fallback
        };
    }

    /// <summary>
    /// Overload for aot
    /// </summary>
    public static async Task<T?> GetSecretAsync<T>(IAmazonSecretsManager client,
                                                   string secretArnOrName,
                                                   JsonTypeInfo<T> jsonTypeInfo,
                                                   string versionStage = "AWSCURRENT",
                                                   CancellationToken cancellationToken = default)
    {
        return await GetSecretAsyncHelper<T>(client, secretArnOrName, jsonTypeInfo, versionStage, cancellationToken);
    }

    /// <summary>
    /// Json Object Version
    /// </summary>
    [RequiresUnreferencedCode("DynamicBehavior is incompatible with trimming. Use Overload with JsonTypeInfo.")]
    public static async Task<T?> GetSecretAsync<T>(IAmazonSecretsManager client,
                                                   string secretArnOrName,
                                                   string versionStage = "AWSCURRENT",
                                                   JsonSerializerOptions? jsonSerializerOptions = null,
                                                   CancellationToken cancellationToken = default)
    {
        var defaultSerialiationOptions = jsonSerializerOptions ?? JsonSerializerOptions.Default;
        
        return await GetSecretAsyncHelper<T>(client, secretArnOrName, defaultSerialiationOptions.GetTypeInfo(typeof(T)), versionStage, cancellationToken);
    }

    private static async Task<T?> GetSecretAsyncHelper<T>(IAmazonSecretsManager client,
                                                  string secretArnOrName,
                                                  JsonTypeInfo jsonTypeInfo,
                                                  string versionStage = "AWSCURRENT",
                                                  CancellationToken cancellationToken = default)
    {
        var temp = await GetSecretAsync(client, secretArnOrName, versionStage, cancellationToken).ConfigureAwait(false);

        return string.IsNullOrEmpty(temp) ?
                    default :
                    (T)(JsonSerializer.Deserialize(temp, jsonTypeInfo) ?? throw new Exception("Can't Convert To Type Of T"));
    }

    private static string StreamToString(MemoryStream secretBinaryToConvert)
    {
        using var reader = new StreamReader(secretBinaryToConvert);
        return Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
    }
}
