using LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler;

public class FhirAuthenticationMessageHandler : HttpClientHandler
{
    public FhirAuthenticationMessageHandler(IFhirBearerTokenProvider fhirBearerTokenProvider)
    {
        FhirBearerTokenProvider = fhirBearerTokenProvider;
    }

    private IFhirBearerTokenProvider FhirBearerTokenProvider { get; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await FhirBearerTokenProvider.AccessTokenAsync(cancellationToken).ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //need to abstract this to a method so we can mock it easier. It's the same method to run and the base to mock out. (SendAsync)
        return await SendToBaseAsync(request, cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    protected virtual async Task<HttpResponseMessage> SendToBaseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await base.SendAsync(request, cancellationToken);
    }
}
