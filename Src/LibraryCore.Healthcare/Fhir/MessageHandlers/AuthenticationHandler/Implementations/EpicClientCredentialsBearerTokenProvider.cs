namespace LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.Implementations;

public class EpicClientCredentialsBearerTokenProvider : IFhirBearerTokenProvider
{
    public ValueTask<string> AccessTokenAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
