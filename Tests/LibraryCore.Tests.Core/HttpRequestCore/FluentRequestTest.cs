using LibraryCore.Core.HttpRequestCore;
using Moq.Protected;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LibraryCore.Tests.Core.HttpRequestCore;

public class FluentRequestTest
{
    public FluentRequestTest()
    {
        MockHttpHandler = new Mock<HttpMessageHandler>();
        HttpClientToUse = new HttpClient(MockHttpHandler.Object);
    }

    private Mock<HttpMessageHandler> MockHttpHandler { get; }
    private HttpClient HttpClientToUse { get; }

    #region Framework

    public record WeatherForecast(int Id, int TemperatureF, string Summary);

    private static HttpResponseMessage CreateMockResponse<T>(HttpStatusCode httpStatusCode, T modelToExpect)
    {
        return new HttpResponseMessage
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(JsonSerializer.Serialize(modelToExpect))
        };
    }

    private void MockHttpRequest(HttpResponseMessage mockResponseToReturn, Expression<Func<HttpRequestMessage, bool>> messageCheck)
    {
        MockHttpHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.Is(messageCheck),
              ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(mockResponseToReturn);
    }

    private void MockHttpRequest(HttpResponseMessage mockResponseToReturn)
    {
        MockHttpRequest(mockResponseToReturn, x => true);
    }

    private void VerifyAndThrow(Times times, Expression<Func<HttpRequestMessage, bool>> expressionToVerify)
    {
        MockHttpHandler.Protected().Verify(
             "SendAsync",
             times,
             ItExpr.Is(expressionToVerify),
             ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Unit Tests

    [Fact]
    public void AddQueryString()
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                .AddQueryString("id", "4");

        Assert.Equal("https://test.api/WeatherForecast?id=4", request.ToMessage().RequestUri!.AbsoluteUri);
    }

    [Fact]
    public void AddMultipleQueryString()
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                .AddQueryString("id", "4")
                                .AddQueryString("id2", "5");

        Assert.Equal("https://test.api/WeatherForecast?id=4&id2=5", request.ToMessage().RequestUri!.AbsoluteUri);
    }

    [Fact]
    public void AddQueryStringWhenUrlHasQueryString()
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast?id100=100")
                                .AddQueryString("id", "4")
                                .AddQueryString("id2", "5");

        Assert.Equal("https://test.api/WeatherForecast?id100=100&id=4&id2=5", request.ToMessage().RequestUri!.AbsoluteUri);
    }

    [InlineData(FluentRequest.AcceptType.Json, "application/json")]
    [InlineData(FluentRequest.AcceptType.TextHtml, "text/html")]
    [Theory]
    public void AddAcceptType(FluentRequest.AcceptType acceptTypeToTest, string expectedValue)
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                .AddAcceptType(acceptTypeToTest);

        Assert.Equal(expectedValue, request.ToMessage().Headers.Accept.ToString());
    }

    [Fact]
    public async Task JsonRequestAndResponse()
    {
        var mockResponse = CreateMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new WeatherForecast(1, 10, "Weather 1")
            });

        MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));

        var jsonParameters = new
        {
            Id = 10,
            Name = "Test"
        };

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddHeader("Header1", "Header1Value")
                                                .AddJsonBody(jsonParameters)
                                                .ToMessage();

        var response = await HttpClientToUse.SendAsync(request);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>() ?? throw new Exception("Can't deserialize result");

        Assert.Equal("application/json", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Equal(JsonSerializer.Serialize(jsonParameters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), await request.Content!.ReadAsStringAsync());
        Assert.Single(result);
        Assert.Contains(result, x => x.Id == 1 && x.TemperatureF == 10 && x.Summary == "Weather 1");

        VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));
    }

    [Fact]
    public async Task FormsEncodedRequestAndResponse()
    {
        var mockResponse = CreateMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new WeatherForecast(1, 10, "Weather 1")
            });

        MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get &&
                                             req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri &&
                                             req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value") &&
                                             req.Headers.Any(t => t.Key == "Header2" && t.Value.First() == "Header2Value"));

        var parameters = new[]
        {
            new KeyValuePair<string,string>("10","Test10"),
            new KeyValuePair<string,string>("20","Test20")
        };

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddHeaders(new[]
                                                {
                                                    new KeyValuePair<string,string>("Header1", "Header1Value"),
                                                    new KeyValuePair<string,string>("Header2", "Header2Value")
                                                })
                                                .AddFormsUrlEncodedBody(parameters)
                                                .ToMessage();

        var response = await HttpClientToUse.SendAsync(request);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>() ?? throw new Exception("Can't deserialize result");

        Assert.Equal("application/x-www-form-urlencoded", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Equal("10=Test10&20=Test20", await request.Content!.ReadAsStringAsync());
        Assert.Single(result);
        Assert.Contains(result, x => x.Id == 1 && x.TemperatureF == 10 && x.Summary == "Weather 1");

        VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get &&
                                            req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri &&
                                            req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value") &&
                                            req.Headers.Any(t => t.Key == "Header2" && t.Value.First() == "Header2Value"));
    }

    [Fact]
    public async Task FileStreamByteArrayRequestAndResponse()
    {
        var mockResponse = CreateMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new WeatherForecast(1, 10, "Weather 1")
            });

        MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);

        var byteArray = new byte[] { 1, 2, 3 };

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddFileStreamBody("test.jpg", byteArray)
                                                .ToMessage();

        var response = await HttpClientToUse.SendAsync(request);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>() ?? throw new Exception("Can't deserialize result");

        Assert.Equal("multipart/form-data", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Contains("--Upload--", await request.Content!.ReadAsStringAsync());
        Assert.Single(result);
        Assert.Contains(result, x => x.Id == 1 && x.TemperatureF == 10 && x.Summary == "Weather 1");

        VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);
    }

    [Fact]
    public async Task FileStreamRequestAndResponse()
    {
        var mockResponse = CreateMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new WeatherForecast(1, 10, "Weather 1")
            });

        MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);

        var byteArrayStream = new MemoryStream(new byte[] { 1, 2, 3 });

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddFileStreamBody("test.jpg", byteArrayStream)
                                                .ToMessage();

        var response = await HttpClientToUse.SendAsync(request);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>() ?? throw new Exception("Can't deserialize result");

        Assert.Equal("multipart/form-data", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Contains("--Upload--", await request.Content!.ReadAsStringAsync());
        Assert.Single(result);
        Assert.Contains(result, x => x.Id == 1 && x.TemperatureF == 10 && x.Summary == "Weather 1");

        VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);
    }

    #endregion

}

