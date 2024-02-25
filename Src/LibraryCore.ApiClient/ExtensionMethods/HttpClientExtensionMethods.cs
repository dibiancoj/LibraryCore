using LibraryCore.ApiClient.ExtensionMethods.Models;
using LibraryCore.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace LibraryCore.ApiClient.ExtensionMethods;

public static class HttpClientExtensionMethods
{
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public static async Task<T?> SendRequestToJsonAsync<T>(this HttpClient httpClient, HttpRequestMessage requestMessage, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
    {
        var rawResponse = await SendMessageHelper(httpClient, requestMessage, cancellationToken);

        return await rawResponse.EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<T>(options: jsonSerializerOptions, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Union type response. If 200 Ok = Deserialize To a model. If a 400 bad request = Deserialize a different model. This way we can handle specific items to a different model type
    /// </summary>
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public static async Task<SendRequestToJsonUnionResult<T1Ok, T2BadRequest>> SendRequestToJsonAsync<T1Ok, T2BadRequest>(this HttpClient httpClient, HttpRequestMessage requestMessage, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
    {
        var rawResponse = await SendMessageHelper(httpClient, requestMessage, cancellationToken);

        if (rawResponse.StatusCode != System.Net.HttpStatusCode.OK && rawResponse.StatusCode != System.Net.HttpStatusCode.BadRequest)
        {
            rawResponse.EnsureSuccessStatusCode();
        }

        return rawResponse.IsSuccessStatusCode ?
                SendRequestToJsonUnionResult<T1Ok, T2BadRequest>.CreateSuccess(await rawResponse.Content.ReadFromJsonAsync<T1Ok>(jsonSerializerOptions, cancellationToken)) :
                SendRequestToJsonUnionResult<T1Ok, T2BadRequest>.CreateBadRequest(await rawResponse.Content.ReadFromJsonAsync<T2BadRequest>(jsonSerializerOptions, cancellationToken));
    }

    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
    public static async Task<T?> SendRequestToXmlAsync<T>(this HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        var rawResponse = await SendMessageHelper(httpClient, requestMessage, cancellationToken);

        return await rawResponse.EnsureSuccessStatusCode().Content.ReadFromXmlAsync<T>(cancellationToken: cancellationToken);
    }

    private static Task<HttpResponseMessage> SendMessageHelper(HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        return httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    public static async Task<Token> TokenAsync(this HttpClient httpClient, string url, string clientId, string clientSecret, JsonTypeInfo<Token> jsonTypeInfo, string grantType = "client_credentials", string scope = "Read", CancellationToken cancellationToken = default)
    {
        var rawResponse = (await httpClient.PostAsync(url, new FormUrlEncodedContent(
        [
            new("client_id", clientId),
            new("client_secret", clientSecret),
            new("grant_type", grantType),
            new("scope", scope)
        ]), cancellationToken))
        .EnsureSuccessStatusCode();

        return await rawResponse.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken: cancellationToken) ?? throw new Exception("Can't Deserialize Token");
    }

    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccessUseOverload)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public static async Task<Token> TokenAsync(this HttpClient httpClient, string url, string clientId, string clientSecret, string grantType = "client_credentials", string scope = "Read", CancellationToken cancellationToken = default)
    {
        return await httpClient.TokenAsync(url, clientId, clientSecret, AotUtilities.ResolveJsonTypeInfo<Token>(), grantType, scope, cancellationToken);
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
        public bool IsExpired(TimeSpan TimeBuffer) => DateTime.Now.Add(-TimeBuffer) >= ExpiresLocalTime;
        public bool IsExpired() => DateTime.Now >= ExpiresLocalTime;
    }
}
