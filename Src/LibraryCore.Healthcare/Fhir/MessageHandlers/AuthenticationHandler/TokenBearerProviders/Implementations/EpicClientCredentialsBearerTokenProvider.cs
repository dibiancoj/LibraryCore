using LibraryCore.Caching;
using LibraryCore.Healthcare.Epic.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders.Implementations;

public class EpicClientCredentialsBearerTokenProvider : IFhirBearerTokenProvider
{
    public EpicClientCredentialsBearerTokenProvider(IMemoryCache memoryCache,
                                                    HttpClient httpClient,
                                                    string tokenEndPointUrl,
                                                    string rawPrivateKeyContentInPemFile,
                                                    string clientId)
    {
        MemoryCache = memoryCache;
        HttpClient = httpClient;
        TokenEndPointUrl = tokenEndPointUrl;
        RawPrivateKeyContentInPemFile = rawPrivateKeyContentInPemFile;
        ClientId = clientId;
    }

    private IMemoryCache MemoryCache { get; }
    private HttpClient HttpClient { get; }
    private string TokenEndPointUrl { get; }
    private string RawPrivateKeyContentInPemFile { get; }
    private string ClientId { get; }

    public async ValueTask<string> AccessTokenAsync(CancellationToken cancellationToken = default)
    {
        return await new InMemoryCacheService(MemoryCache).GetOrCreateWithLockAsync(nameof(EpicClientCredentialsBearerTokenProvider), async entry =>
        {
            var clientAssertion = ClientCredentialsAuthentication.CreateEpicClientAssertionJwtToken(RawPrivateKeyContentInPemFile, ClientId, TokenEndPointUrl);

            var tokenResult = await ClientCredentialsAuthentication.TokenAsync(HttpClient, TokenEndPointUrl, clientAssertion, cancellationToken);

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResult.ExpiresIn);

            return tokenResult.AccessToken;
        }) ?? throw new Exception("Can't Find Token From Cache Or Source");
    }
}
