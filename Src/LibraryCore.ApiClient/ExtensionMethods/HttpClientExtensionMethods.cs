using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.ApiClient.ExtensionMethods;

public static class HttpClientExtensionMethods
{
    public static async Task<T?> SendRequestToJsonAsync<T>(this HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        var rawResponse = await SendMessageHelper(httpClient, requestMessage, cancellationToken);

        return await rawResponse.EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    public static async Task<T?> SendRequestToXmlAsync<T>(this HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        var rawResponse = await SendMessageHelper(httpClient, requestMessage, cancellationToken);

        return await rawResponse.EnsureSuccessStatusCode().Content.ReadFromXmlAsync<T>(cancellationToken: cancellationToken);
    }

    private static Task<HttpResponseMessage> SendMessageHelper(HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        return httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    public static async Task<Token> TokenAsync(this HttpClient httpClient, Uri tokenUri, string clientId, string clientSecret, string grantType = "client_credentials", string scope = "Read", CancellationToken cancellationToken = default)
    {
        return await httpClient.TokenAsync(tokenUri.AbsoluteUri, clientId, clientSecret, grantType, scope, cancellationToken);
    }

    public static async Task<Token> TokenAsync(this HttpClient httpClient, string url, string clientId, string clientSecret, string grantType = "client_credentials", string scope = "Read", CancellationToken cancellationToken = default)
    {
        var rawResponse = await httpClient.PostAsync(url, new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("client_id", clientId),
            new("client_secret", clientSecret),
            new("grant_type", grantType),
            new("scope", scope),
        }), cancellationToken).ConfigureAwait(false);

        return await rawResponse.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<Token>(cancellationToken: cancellationToken).ConfigureAwait(false) ?? throw new Exception("Can't Deserialize Token");
    }

    /// <summary>
    /// Client Credentials Response
    /// </summary>
    /// <param name="ExpiresIn">The remaining time in seconds that the consent remains valid</param>
    /// <param name="ConsentedOn">The time in seconds since the epoch when the resource owner gave consent to the client</param>
    public record Token([property: JsonPropertyName("token_type")] string TokenType,
                        [property: JsonPropertyName("access_token")] string AccessToken,
                        [property: JsonPropertyName("scope")] string Scope,
                        [property: JsonPropertyName("expires_in")] int ExpiresIn,
                        [property: JsonPropertyName("consented_on")] long ConsentedOn)
    {
        public DateTimeOffset ExpiresLocalTime { get; } = DateTimeOffset.FromUnixTimeSeconds(ConsentedOn).ToLocalTime().AddSeconds(ExpiresIn);
    }
}
