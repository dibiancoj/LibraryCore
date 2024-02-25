using LibraryCore.Caching;
using LibraryCore.Healthcare.Epic.Authentication;
using LibraryCore.Shared;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders.Implementations;

[RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
[RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
public class EpicClientCredentialsBearerTokenProvider(IMemoryCache memoryCache,
                                                     HttpClient httpClient,
                                                     string tokenEndPointUrl,
                                                     string rawPrivateKeyContentInPemFile,
                                                     string clientId) : IFhirBearerTokenProvider
{
    private static TimeSpan CacheBufferTimePeriod { get; } = new TimeSpan(0, 1, 0);

    public async ValueTask<string> AccessTokenAsync(CancellationToken cancellationToken = default)
    {
        return await new InMemoryCacheService(memoryCache).GetOrCreateWithLockAndEvictionAsync(nameof(EpicClientCredentialsBearerTokenProvider), async entry =>
        {
            var clientAssertion = ClientCredentialsAuthentication.CreateEpicClientAssertionJwtToken(rawPrivateKeyContentInPemFile, clientId, tokenEndPointUrl);

            var tokenResult = await ClientCredentialsAuthentication.TokenAsync(httpClient, tokenEndPointUrl, clientAssertion, cancellationToken);

            //giving it a minute buffer so we don't get too close to the expiration
            var buffer = tokenResult.ExpiresIn <= CacheBufferTimePeriod.TotalSeconds ?
                                new TimeSpan(0, 0, 0) :
                                CacheBufferTimePeriod;

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResult.ExpiresIn).Subtract(buffer);

            return tokenResult.AccessToken;
        }, cancellationToken) ?? throw new Exception("Can't Find Token From Cache Or Source");
    }
}
