using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using LibraryCore.Shared;
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
    public static async Task<IDictionary<TKey, TValue>?> GetSecretKeyValuePairAsync<TKey, TValue>(
                                                                     IAmazonSecretsManager client,
                                                                     string secretArnOrName,
                                                                     JsonTypeInfo<Dictionary<TKey, TValue>> jsonTypeInfo,
                                                                     string versionStage = "AWSCURRENT",
                                                                     CancellationToken cancellationToken = default)
           where TKey : notnull
    {
        return await GetSecretAsyncHelper(client,
                                          secretArnOrName,
                                          jsonTypeInfo,
                                          versionStage,
                                          cancellationToken);
    }

#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccessUseOverload)]
#endif
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccessUseOverload)]
    public static async Task<IDictionary<TKey, TValue>?> GetSecretKeyValuePairAsync<TKey, TValue>(
                                                                         IAmazonSecretsManager client,
                                                                         string secretArnOrName,
                                                                         string versionStage = "AWSCURRENT",
                                                                         JsonSerializerOptions? jsonSerializerOptions = null,
                                                                         CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        return await GetSecretAsyncHelper(client,
                                          secretArnOrName,
                                          AotUtilities.ResolveJsonTypeInfo<Dictionary<TKey, TValue>>(),
                                          versionStage,
                                          cancellationToken);
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
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccessUseOverload)]
#endif
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccessUseOverload)]
    public static async Task<T?> GetSecretAsync<T>(IAmazonSecretsManager client,
                                                   string secretArnOrName,
                                                   string versionStage = "AWSCURRENT",
                                                   JsonSerializerOptions? jsonSerializerOptions = null,
                                                   CancellationToken cancellationToken = default)
    {
        return await GetSecretAsyncHelper<T>(client, secretArnOrName, AotUtilities.ResolveJsonTypeInfo<T>(), versionStage, cancellationToken);
    }

    private static async Task<T?> GetSecretAsyncHelper<T>(IAmazonSecretsManager client,
                                                          string secretArnOrName,
                                                          JsonTypeInfo<T> jsonTypeInfo,
                                                          string versionStage = "AWSCURRENT",
                                                          CancellationToken cancellationToken = default)
    {
        var temp = await GetSecretAsync(client, secretArnOrName, versionStage, cancellationToken).ConfigureAwait(false);

        return string.IsNullOrEmpty(temp) ?
                    default :
                    JsonSerializer.Deserialize(temp, jsonTypeInfo) ?? throw new Exception("Can't Convert To Type Of T");
    }

    private static string StreamToString(MemoryStream secretBinaryToConvert)
    {
        using var reader = new StreamReader(secretBinaryToConvert);
        return Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
    }
}
