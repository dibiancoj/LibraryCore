namespace LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders;

public interface IFhirBearerTokenProvider
{
    public ValueTask<string> AccessTokenAsync(CancellationToken cancellationToken = default);
}
