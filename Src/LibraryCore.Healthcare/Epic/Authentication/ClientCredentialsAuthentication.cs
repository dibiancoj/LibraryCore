using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Healthcare.Epic.Authentication;

public static class ClientCredentialsAuthentication
{
    /// <summary>
    /// Retrieve a client credentials access token from epic
    /// </summary>
    /// <param name="tokenEndpoint">Token endpoint. Sandbox = https://fhir.epic.com/interconnect-fhir-oauth/oauth2/token</param>
    /// <param name="clientAssertion">This is the created jwt token you generate from the private key</param>
    public static async Task<EpicClientCredentialsAuthorizationToken> TokenAsync(HttpClient httpClient,
                                                                                 string tokenEndpoint,
                                                                                 string clientAssertion,
                                                                                 CancellationToken cancellationToken = default)
    {
        var rawResponse = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("grant_type", "client_credentials"),
            new("client_assertion_type","urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
            new("client_assertion", clientAssertion),
        }), cancellationToken).ConfigureAwait(false);

        return await rawResponse.EnsureSuccessStatusCode()
                                .Content
                                .ReadFromJsonAsync<EpicClientCredentialsAuthorizationToken>(cancellationToken: cancellationToken)
                                .ConfigureAwait(false) ?? throw new Exception("Can't Deserialize Token");
    }
}

public record EpicClientCredentialsAuthorizationToken([property: JsonPropertyName("token_type")] string TokenType,
                                                      [property: JsonPropertyName("access_token")] string AccessToken,
                                                      [property: JsonPropertyName("scope")] string Scope,
                                                      [property: JsonPropertyName("expires_in")] int ExpiresIn)
{
    public DateTimeOffset ExpiresLocalTime { get; } = DateTimeOffset.Now.ToLocalTime().AddSeconds(ExpiresIn);
    public bool IsExpired(TimeSpan TimeBuffer) => DateTime.Now.Add(-TimeBuffer) >= ExpiresLocalTime;
    public bool IsExpired() => DateTime.Now >= ExpiresLocalTime;
    public IEnumerable<string> Scopes() => Scope.Split(' ');
}