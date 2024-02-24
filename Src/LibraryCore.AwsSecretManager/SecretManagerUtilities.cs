using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text;
using System.Text.Json;

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

    public static async Task<IDictionary<TKey, TValue>> GetSecretKeyValuePairAsync<TKey, TValue>(
                                                                         IAmazonSecretsManager client,
                                                                         string secretArnOrName,
                                                                         string versionStage = "AWSCURRENT",
                                                                         JsonSerializerOptions? jsonSerializerOptions = null,
                                                                         CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var temp = await GetSecretAsync(client, secretArnOrName, versionStage, cancellationToken) ?? throw new Exception("Secret Content Is Null From Secret Store");

        return JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(temp, jsonSerializerOptions) ?? throw new Exception("Can't Deserialize To Dictionary");
    }

    /// <summary>
    /// Json Object Version
    /// </summary>
    public static async Task<T?> GetSecretAsync<T>(IAmazonSecretsManager client,
                                                   string secretArnOrName,
                                                   string versionStage = "AWSCURRENT",
                                                   JsonSerializerOptions? jsonSerializerOptions = null,
                                                   CancellationToken cancellationToken = default)
    {
        var temp = await GetSecretAsync(client, secretArnOrName, versionStage, cancellationToken).ConfigureAwait(false);

        return string.IsNullOrEmpty(temp) ?
                    default :
                    JsonSerializer.Deserialize<T>(temp, jsonSerializerOptions);
    }

    private static string StreamToString(MemoryStream secretBinaryToConvert)
    {
        using var reader = new StreamReader(secretBinaryToConvert);
        return Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
    }
}
