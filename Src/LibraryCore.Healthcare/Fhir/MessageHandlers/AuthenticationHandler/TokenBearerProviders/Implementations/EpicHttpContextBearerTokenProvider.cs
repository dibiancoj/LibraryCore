using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders.Implementations;

/// <summary>
/// Used for patient, clinician, etc. This is for open id connect and grabbing from http context
/// </summary>
public class EpicHttpContextBearerTokenProvider(IHttpContextAccessor httpContextAccessor) : IFhirBearerTokenProvider
{
    public async ValueTask<string> AccessTokenAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor.HttpContext);

        return await httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken) ?? throw new Exception("Can't Find Access Token In HttpContext");
    }
}
