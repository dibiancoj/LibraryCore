namespace LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler;

public interface IFhirBearerTokenProvider
{
    ValueTask<string> AccessTokenAsync(CancellationToken cancellationToken = default);
}
