using LibraryCore.Healthcare.Epic.Fhir.BulkExport;
using LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders;
using Moq;
using Moq.Protected;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static LibraryCore.Healthcare.Epic.Fhir.BulkExport.EpicBulkFhirExportApi;

namespace LibraryCore.Tests.Healthcare.Epic.Fhir;

public class EpicBulkFhirExportApiTest
{
    private Mock<HttpMessageHandler> MockHttpHandler { get; }
    private HttpClient HttpClient { get; }
    private Mock<IFhirBearerTokenProvider> FhirBearerTokenProvider { get; }
    private EpicBulkFhirExportApi EpicBulkFhirExportApiToUse { get; }

    public EpicBulkFhirExportApiTest()
    {
        MockHttpHandler = new Mock<HttpMessageHandler>();
        HttpClient = new HttpClient(MockHttpHandler.Object);
        FhirBearerTokenProvider = new Mock<IFhirBearerTokenProvider>();
        EpicBulkFhirExportApiToUse = new EpicBulkFhirExportApi(HttpClient, FhirBearerTokenProvider.Object);
    }

    #region Framework

    private void SetupFhirBearerTokenProvider()
    {
        FhirBearerTokenProvider.Setup(x => x.AccessTokenAsync(It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<string>("abc"));
    }

    private static HttpResponseMessage CreateMockedResponse<T>(T? responseModel, HttpStatusCode code = HttpStatusCode.OK)
    {
        return new HttpResponseMessage
        {
            StatusCode = code,
            Content = responseModel == null ? null : new StringContent(JsonSerializer.Serialize(responseModel), Encoding.UTF8, "application/json")
        };
    }

    private void CreateMockedHttpHandlerCall(HttpResponseMessage mockedResponse, Expression<Func<HttpRequestMessage, bool>> predicate)
    {
        MockHttpHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                 ItExpr.Is(predicate),
                 ItExpr.IsAny<CancellationToken>())
              .ReturnsAsync(mockedResponse);
    }

    private void CreateVerifyHttpHandlerCall(Expression<Func<HttpRequestMessage, bool>> predicate)
    {
        MockHttpHandler.Protected()
            .Verify(
                 nameof(HttpClient.SendAsync),
                 Times.Once(),
                 ItExpr.Is(predicate),
                 ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    [Fact]
    public async Task KickOffExportTest()
    {
        SetupFhirBearerTokenProvider();

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockResponse.Content.Headers.Add("Content-Location", "MyLocationOfContent");

        Expression<Func<HttpRequestMessage, bool>> msgChecker = msg => msg.Headers.GetValues("Prefer").First().Equals("respond-async") &&
                                                                       msg.Headers.Authorization!.Scheme == "Bearer" &&
                                                                       msg.Headers.Authorization!.Parameter == "abc";

        CreateMockedHttpHandlerCall(mockResponse, msgChecker);

        var result = await EpicBulkFhirExportApiToUse.KickOffBulkRequestAsync("http://server.fhir/group/zzzzzz/$export");

        Assert.Equal("MyLocationOfContent", result.ContentLocation);

        FhirBearerTokenProvider.VerifyAll();
        CreateVerifyHttpHandlerCall(msgChecker);
    }

    [Fact]
    public async Task CheckStatusNotDoneTest()
    {
        Assert.True(false);
    }

    [Fact]
    public async Task CheckStatusIsDoneTest()
    {
        Assert.True(false);
    }

    [Fact]
    public async Task CompletedResultTest()
    {
        Assert.True(false);
    }

    [Fact]
    public async Task FullApiFlowTest()
    {
        Assert.True(false);
    }
}
