using LibraryCore.ApiClient;
using LibraryCore.ApiClient.ExtensionMethods;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

    private static ReadOnlySpan<byte> ClensePrivateKey(ReadOnlySpan<char> rawContentPrivateKeyPem)
    {
        //there is a perf test with this code and a string builder, regex, and a few other patterns to avoid allocation and fastest code.

        //basic method without all the fancyiness is just taking everything between the "begin private key" header and the "end private key" footer

        //Find the index after the header which has the -----BEGIN PRIVATE KEY-----
        var indexAfterHeaderContent = rawContentPrivateKeyPem.IndexOf(Environment.NewLine);

        //grab the last line break which has the -----END PRIVATE KEY-----
        var lastPageIndex = rawContentPrivateKeyPem.LastIndexOf(Environment.NewLine);

        //Most performant version but a bit crazy
        //grab everything between BEGIN PRIVATE KEY and END PRIVATE KEY
        //var contentBetweenHeaderAndFooter = rawContentPrivateKeyPem[indexAfterHeaderContent..lastPageIndex];

        ////going to get fancy and try not to allocate anything (in base64 every char encodes 6 bits, so 4 chars = 3 bytes)
        //var buffer = new Span<byte>(new byte[((contentBetweenHeaderAndFooter.Length * 3) + 3) / 4]);

        ////pem file is in base 64...so decode it back to bytes using the buffer
        //if (!Convert.TryFromBase64Chars(contentBetweenHeaderAndFooter, buffer, out _))
        //{
        //    throw new Exception("Cant' Convert From Base 64");
        //}

        //return buffer;
        //end of most performant code

        //this is the simplified version instead of using the buffer
        return Convert.FromBase64String(new string(rawContentPrivateKeyPem[indexAfterHeaderContent..lastPageIndex]));
    }
}

public record EpicClientCredentialsAuthorizationToken([property: JsonPropertyName("token_type")] string TokenType,
                                                      [property: JsonPropertyName("access_token")] string AccessToken,
                                                      [property: JsonPropertyName("scope")] string Scope,
                                                      [property: JsonPropertyName("expires_in")] int ExpiresIn)
{
    public DateTimeOffset ExpiresLocalTime { get; } = DateTimeOffset.Now.ToLocalTime().AddSeconds(ExpiresIn);
    public bool IsExpired(TimeSpan TimeBuffer) => DateTime.Now.Add(-TimeBuffer) >= ExpiresLocalTime;
    public IReadOnlyCollection<string> Scopes = Scope.Split(' ').ToImmutableArray();
    public bool IsExpired() => DateTime.Now >= ExpiresLocalTime;
}