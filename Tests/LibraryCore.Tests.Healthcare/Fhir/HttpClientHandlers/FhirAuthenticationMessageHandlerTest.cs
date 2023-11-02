using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler;
using Moq.Protected;
using System.Text;
using System.Text.Json;

namespace LibraryCore.Tests.Healthcare.Fhir.HttpClientHandlers;

public class FhirAuthenticationMessageHandlerTest
{
    [Fact]
    public async System.Threading.Tasks.Task FhirAddsBearerTokenTest()
    {
        var tokenValue = Guid.NewGuid().ToString();
        var mockTokenProvider = new Mock<IFhirBearerTokenProvider>();
        var handler = new Mock<FhirAuthenticationMessageHandler>(mockTokenProvider.Object) { CallBase = true };
        var httpClient = new HttpClient(handler.Object);
        var fhirClient = new FhirClient("http://myfhirserver", httpClient, new FhirClientSettings());

        mockTokenProvider.Setup(x => x.AccessTokenAsync(It.IsAny<CancellationToken>()))
          .Returns(new ValueTask<string>(tokenValue));

        Patient mockPatientResponse = new() { Id = "12345678"};
        var options = new JsonSerializerOptions().ForFhir(ModelInfo.ModelInspector).Pretty();
        string patientJson = JsonSerializer.Serialize(mockPatientResponse, options);

        var mockResponse = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,

            Content = new StringContent(patientJson, Encoding.UTF8, "application/json")
        };

        handler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
                "SendToBaseAsync",
                ItExpr.Is<HttpRequestMessage>(msg => msg.Headers.Authorization!.Scheme == "Bearer" && msg.Headers.Authorization.Parameter == tokenValue),
                ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(mockResponse);

        var patientResult = await fhirClient.ReadAsync<Patient>("/Patient/12345678");

        handler.Protected().Verify(
            "SendToBaseAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(msg => msg.Headers.Authorization!.Scheme == "Bearer" && msg.Headers.Authorization.Parameter == tokenValue),
            ItExpr.IsAny<CancellationToken>());

        Assert.Equal("12345678", patientResult!.Id);

        mockTokenProvider.VerifyAll();
    }
}
