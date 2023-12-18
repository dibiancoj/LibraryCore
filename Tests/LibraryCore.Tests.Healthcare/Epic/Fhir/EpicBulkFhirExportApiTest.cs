using Hl7.Fhir.Serialization;
using LibraryCore.Healthcare.Epic.Fhir.BulkExport;
using LibraryCore.Healthcare.Epic.Fhir.BulkExport.Models;
using LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders;
using Moq.Protected;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Json;
using static LibraryCore.Healthcare.Epic.Fhir.BulkExport.Models.BulkFhirCompletedStatus;

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
        string serializedContent = responseModel is string castedString ? castedString : JsonSerializer.Serialize(responseModel);

        return new HttpResponseMessage
        {
            StatusCode = code,
            Content = responseModel == null ? null : new StringContent(serializedContent, Encoding.UTF8, "application/json")
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

    private void CreateVerifyHttpHandlerCall(Expression<Func<HttpRequestMessage, bool>> predicate, Times? times = default)
    {
        MockHttpHandler.Protected()
            .Verify(
                 nameof(HttpClient.SendAsync),
                 times == null ? Times.Once() : times.Value,
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
        SetupFhirBearerTokenProvider();

        var mockResponse = new HttpResponseMessage(HttpStatusCode.Accepted);

        mockResponse.Headers.Add("X-Progress", "10 of 100 Patients Checked");

        Expression<Func<HttpRequestMessage, bool>> msgChecker = msg => msg.Headers.Authorization!.Scheme == "Bearer" &&
                                                                       msg.Headers.Authorization!.Parameter == "abc";

        CreateMockedHttpHandlerCall(mockResponse, msgChecker);

        var result = await EpicBulkFhirExportApiToUse.CheckStatusOfBulkRequestAsync("https://MyLocationOfContent");

        var castedResult = Assert.IsType<BulkFhirInProgressStatus>(result);
        Assert.Equal("10 of 100 Patients Checked", castedResult.ProgressDescription);

        FhirBearerTokenProvider.VerifyAll();
        CreateVerifyHttpHandlerCall(msgChecker);
    }

    [Fact]
    public async Task CheckStatusIsDoneTest()
    {
        SetupFhirBearerTokenProvider();

        var mockResultOutput = new BulkFhirCompletedResultOutput[]
        {
            new("Patient", "Http://Patient/1.json"),
            new("Patient", "Http://Patient/2.json")
        };

        var mockResponse = CreateMockedResponse(new BulkFhirCompletedResult(
                                                            DateTime.Now,
                                                            "MyLocationOfContent",
                                                            true,
                                                            mockResultOutput,
                                                            []));


        Expression<Func<HttpRequestMessage, bool>> msgChecker = msg => msg.Headers.Authorization!.Scheme == "Bearer" &&
                                                                       msg.Headers.Authorization!.Parameter == "abc";

        CreateMockedHttpHandlerCall(mockResponse, msgChecker);

        var result = await EpicBulkFhirExportApiToUse.CheckStatusOfBulkRequestAsync("https://MyLocationOfContent");

        var castedResult = Assert.IsType<BulkFhirCompletedStatus>(result);

        Assert.Contains(castedResult.Result.Output, x => x.Type == "Patient" && x.Url == "Http://Patient/1.json");
        Assert.Contains(castedResult.Result.Output, x => x.Type == "Patient" && x.Url == "Http://Patient/2.json");

        FhirBearerTokenProvider.VerifyAll();
        CreateVerifyHttpHandlerCall(msgChecker);
    }

    [Fact]
    public async Task CompletedResultTest()
    {
        SetupFhirBearerTokenProvider();

        var urls = new string[]
        {
            "http://fhir/Patient/file1.json",
            "http://fhir/Patient/file2.json",
        };

        var mockPatient1 = JsonSerializer.Serialize(new Hl7.Fhir.Model.Patient { Id = "11111111" }, new JsonSerializerOptions().ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector));
        var mockPatient2 = JsonSerializer.Serialize(new Hl7.Fhir.Model.Patient { Id = "22222222" }, new JsonSerializerOptions().ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector));
        var mockPatient3 = JsonSerializer.Serialize(new Hl7.Fhir.Model.Patient { Id = "33333333" }, new JsonSerializerOptions().ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector));

        var mockResponse1 = CreateMockedResponse(string.Join(Environment.NewLine, new[] { mockPatient1, mockPatient2 }));
        var mockResponse2 = CreateMockedResponse(string.Join(Environment.NewLine, new[] { mockPatient3 }));

        Expression<Func<HttpRequestMessage, bool>> msgChecker = msg => msg.Headers.Authorization!.Scheme == "Bearer" &&
                                                                       msg.Headers.Authorization!.Parameter == "abc";

        MockHttpHandler
          .Protected()
          .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is(msgChecker),
                ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(mockResponse1)
             .ReturnsAsync(mockResponse2);

        var patientsFromData = new List<Hl7.Fhir.Model.Patient>();

        await foreach (var patientRecord in EpicBulkFhirExportApiToUse.BulkResultRawSectionData<Hl7.Fhir.Model.Patient>(urls))
        {
            patientsFromData.Add(patientRecord);
        }

        Assert.Contains(patientsFromData, x => x.Id == "11111111");
        Assert.Contains(patientsFromData, x => x.Id == "22222222");
        Assert.Contains(patientsFromData, x => x.Id == "33333333");

        FhirBearerTokenProvider.VerifyAll();
        CreateVerifyHttpHandlerCall(msgChecker, Times.Exactly(2));
    }

    [Fact]
    public async Task CompletedResultTestWithCancelInMiddle()
    {
        SetupFhirBearerTokenProvider();

        var urls = new string[]
        {
            "http://fhir/Patient/file1.json",
            "http://fhir/Patient/file2.json",
        };

        var mockPatient1 = JsonSerializer.Serialize(new Hl7.Fhir.Model.Patient { Id = "11111111" }, new JsonSerializerOptions().ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector));
        var mockPatient2 = JsonSerializer.Serialize(new Hl7.Fhir.Model.Patient { Id = "22222222" }, new JsonSerializerOptions().ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector));
        var mockPatient3 = JsonSerializer.Serialize(new Hl7.Fhir.Model.Patient { Id = "33333333" }, new JsonSerializerOptions().ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector));

        var mockResponse1 = CreateMockedResponse(string.Join(Environment.NewLine, new[] { mockPatient1, mockPatient2 }));
        var mockResponse2 = CreateMockedResponse(string.Join(Environment.NewLine, new[] { mockPatient3 }));

        Expression<Func<HttpRequestMessage, bool>> msgChecker = msg => msg.Headers.Authorization!.Scheme == "Bearer" &&
                                                                       msg.Headers.Authorization!.Parameter == "abc";

        MockHttpHandler
          .Protected()
          .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is(msgChecker),
                ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(mockResponse1)
             .ReturnsAsync(mockResponse2);

        var patientsFromData = new List<Hl7.Fhir.Model.Patient>();
        var cancelTokenSource = new CancellationTokenSource();

        await foreach (var patientRecord in EpicBulkFhirExportApiToUse.BulkResultRawSectionData<Hl7.Fhir.Model.Patient>(urls, cancelTokenSource.Token))
        {
            patientsFromData.Add(patientRecord);

            if (patientsFromData.Count == 1)
            {
                cancelTokenSource.Cancel();
            }
        }

        Assert.Single(patientsFromData);
        Assert.Contains(patientsFromData, x => x.Id == "11111111");

        FhirBearerTokenProvider.VerifyAll();
        CreateVerifyHttpHandlerCall(msgChecker, Times.Once());
    }

    [Fact]
    public async Task DeleteFhirData()
    {
        SetupFhirBearerTokenProvider();

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);

        mockResponse.Content.Headers.Add("Content-Location", "MyLocationOfContent");

        Expression<Func<HttpRequestMessage, bool>> msgChecker = msg => msg.Headers.Authorization!.Scheme == "Bearer" &&
                                                                       msg.Headers.Authorization!.Parameter == "abc";

        CreateMockedHttpHandlerCall(mockResponse, msgChecker);

        await EpicBulkFhirExportApiToUse.DeleteBulkRequestAsync("http://server.fhir/group/zzzzzz/$export");

        FhirBearerTokenProvider.VerifyAll();
        CreateVerifyHttpHandlerCall(msgChecker);
    }

    [Fact]
    public async Task FullApiFlowTest()
    {
        SetupFhirBearerTokenProvider();

        var kickOffMockMessageResponse = new HttpResponseMessage(HttpStatusCode.OK);
        kickOffMockMessageResponse.Content.Headers.Add("Content-Location", "http://fhir/export/123");

        var mockResponseForStatusPending = new HttpResponseMessage(HttpStatusCode.Accepted);
        mockResponseForStatusPending.Headers.Add("X-Progress", "10 of 100 Patients Checked");

        var mockResponseForStatusCompleted = CreateMockedResponse(new BulkFhirCompletedResult(
                                                            DateTime.Now,
                                                            "http://fhir/export/123",
                                                            true,
                                                            [
                                                                new("Patient", "Http://Patient/1.json"),
                                                                new("Patient", "Http://Patient/2.json")
                                                            ],
                                                            []));

        var mockPatient1 = JsonSerializer.Serialize(new Hl7.Fhir.Model.Patient { Id = "11111111" }, new JsonSerializerOptions().ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector));
        var mockPatient2 = JsonSerializer.Serialize(new Hl7.Fhir.Model.Patient { Id = "22222222" }, new JsonSerializerOptions().ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector));
        var mockPatient3 = JsonSerializer.Serialize(new Hl7.Fhir.Model.Patient { Id = "33333333" }, new JsonSerializerOptions().ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector));

        var mockGetDetailsWhenDone1 = CreateMockedResponse(string.Join(Environment.NewLine, new[] { mockPatient1, mockPatient2 }));
        var mockGetDetailsWhenDone2 = CreateMockedResponse(string.Join(Environment.NewLine, new[] { mockPatient3 }));

        MockHttpHandler
         .Protected()
         .SetupSequence<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(kickOffMockMessageResponse)
            .ReturnsAsync(mockResponseForStatusPending)
            .ReturnsAsync(mockResponseForStatusCompleted)
            .ReturnsAsync(mockGetDetailsWhenDone1)
            .ReturnsAsync(mockGetDetailsWhenDone2);

        var results = new List<Hl7.Fhir.Model.Patient>();

        await foreach (var patientResult in EpicBulkFhirExportApiToUse.KickOffAndWaitForCompletionAsync<Hl7.Fhir.Model.Patient>("http://server.fhir/group/zzzzzz/$export", false, TimeSpan.FromSeconds(2)))
        {
            results.Add(patientResult);
        }

        Assert.Equal(3, results.Count);
        Assert.Contains(results, x => x.Id == "11111111");
        Assert.Contains(results, x => x.Id == "22222222");
        Assert.Contains(results, x => x.Id == "33333333");

        FhirBearerTokenProvider.VerifyAll();
    }
}
