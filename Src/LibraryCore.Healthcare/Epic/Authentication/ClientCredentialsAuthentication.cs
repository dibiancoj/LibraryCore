using LibraryCore.ApiClient;
using LibraryCore.ApiClient.ExtensionMethods;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace LibraryCore.Healthcare.Epic.Authentication;

public static class ClientCredentialsAuthentication
{
    /// <summary>
    /// Retrieve a client credentials access token from epic
    /// </summary>
    /// <param name="tokenEndPointUrl">Token endpoint. Sandbox = https://fhir.epic.com/interconnect-fhir-oauth/oauth2/token</param>
    /// <param name="clientAssertion">This is the created jwt token you generate from the private key</param>
    public static async Task<EpicClientCredentialsAuthorizationToken> TokenAsync(HttpClient httpClient,
                                                                                 string tokenEndPointUrl,
                                                                                 string clientAssertion,
                                                                                 CancellationToken cancellationToken = default)
    {
        var request = new FluentRequest(HttpMethod.Post, tokenEndPointUrl)
                            .AddFormsUrlEncodedBody(new("grant_type", "client_credentials"),
                                                    new("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                                                    new("client_assertion", clientAssertion));

        return await httpClient.SendRequestToJsonAsync<EpicClientCredentialsAuthorizationToken>(request, cancellationToken: cancellationToken) ?? throw new Exception("Can't Deserialize Token");
    }

    /// <summary>
    /// Creates a jwt token from a private key. This is needed to pass an client assertion value into TokenAsync.
    /// </summary>
    /// <param name="rawPrivateKeyContentInPemFile">The full content in the pem file. The header and footer value will be cleansed in this method</param>
    /// <param name="clientId">Client id for epic</param>
    /// <param name="tokenEndPointUrl">Token endpoint. Sandbox = https://fhir.epic.com/interconnect-fhir-oauth/oauth2/token</param>
    /// <returns>Jwt token which is passed into TokenAsync for client assertion</returns>
    public static string CreateEpicClientAssertionJwtToken(string rawPrivateKeyContentInPemFile, string clientId, string tokenEndPointUrl)
    {
        var now = DateTime.UtcNow;
        Guid gJti = Guid.NewGuid(); // you may improve it

        var provider = new RSACryptoServiceProvider(); //don't dispose of this will get "Safe handle has been closed". Because its being disposed as soon as we init the RsaSecurity Key. So we need to keep it alive longer. It disposes of it when it shouldn't.

        provider.ImportPkcs8PrivateKey(ClensePrivateKey(rawPrivateKeyContentInPemFile), out _);

        var jwtToken = new JwtSecurityToken
        (
            issuer: clientId,
            audience: tokenEndPointUrl,
            claims: new Claim[]
            {
                new(JwtRegisteredClaimNames.Sub, clientId),
                new(JwtRegisteredClaimNames.Jti, gJti.ToString())
            },
            notBefore: now.AddMilliseconds(-30),
            expires: now.AddMinutes(4), //shouldn't be more then 5 minutes. This is the time it takes for authentication on this jwt to get an access token. Not for the consumer to make an api call and authenticate that token
            signingCredentials: new SigningCredentials(new RsaSecurityKey(provider), SecurityAlgorithms.RsaSha384)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    private static ReadOnlySpan<byte> ClensePrivateKey(string rawContentPrivateKeyPem)
    {
        //use a span to optimize this
        var builder = new StringBuilder(rawContentPrivateKeyPem)
                        .Replace("-----BEGIN PRIVATE KEY-----", string.Empty)
                        .Replace("-----END PRIVATE KEY-----", string.Empty);

        return Convert.FromBase64String(builder.ToString());
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