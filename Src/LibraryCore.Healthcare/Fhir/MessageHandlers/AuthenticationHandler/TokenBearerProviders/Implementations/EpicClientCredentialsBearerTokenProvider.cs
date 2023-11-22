using LibraryCore.Caching;
using LibraryCore.Healthcare.Epic.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders.Implementations;

public class EpicClientCredentialsBearerTokenProvider(IMemoryCache memoryCache,
                                                     HttpClient httpClient,
                                                     string tokenEndPointUrl,
                                                     string rawPrivateKeyContentInPemFile,
                                                     string clientId) : IFhirBearerTokenProvider
{
    public async ValueTask<string> AccessTokenAsync(CancellationToken cancellationToken = default)
    {
        return await new InMemoryCacheService(memoryCache).GetOrCreateWithLockAsync(nameof(EpicClientCredentialsBearerTokenProvider), async entry =>
        {
            var clientAssertion = ClientCredentialsAuthentication.CreateEpicClientAssertionJwtToken(rawPrivateKeyContentInPemFile, clientId, tokenEndPointUrl);

            var tokenResult = await ClientCredentialsAuthentication.TokenAsync(httpClient, tokenEndPointUrl, clientAssertion, cancellationToken);

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResult.ExpiresIn);

            return tokenResult.AccessToken;
        }) ?? throw new Exception("Can't Find Token From Cache Or Source");
    }
}
