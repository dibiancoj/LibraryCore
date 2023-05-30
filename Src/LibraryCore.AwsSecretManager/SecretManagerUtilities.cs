using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text;
using System.Text.Json;

namespace LibraryCore.AwsSecretManager;

public static class SecretManagerUtilities
{
    //AmazonSecretsManagerClient is the IAmazonSecretsManager implementation.
    //In DI services.AddAWSService<IAmazonSecretsManager>();
    //notes: Doesn't have support to get a specific version. Not needed so VersionId is not a parameter in the method
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

        if (tempResponse.SecretString is not null)
        {
            return tempResponse.SecretString;
        }

        if (tempResponse.SecretBinary is not null)
        {
            using var reader = new StreamReader(tempResponse.SecretBinary);
            return Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
        }

        return null;
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

        if (string.IsNullOrEmpty(temp))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(temp, jsonSerializerOptions);
    }
}
