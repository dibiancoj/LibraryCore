namespace LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders;

public interface IFhirBearerTokenProvider
{
    ValueTask<string> AccessTokenAsync(CancellationToken cancellationToken = default);
}
