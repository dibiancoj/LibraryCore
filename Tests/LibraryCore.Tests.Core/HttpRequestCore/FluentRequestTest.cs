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

        var result = await HttpClientToUse.SendAsync(request);

        var response = await result.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>() ?? throw new Exception("Can't deserialize result");

        Assert.Equal("application/json", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Equal(JsonSerializer.Serialize(jsonParameters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), await request.Content!.ReadAsStringAsync());
        Assert.Single(response);
        Assert.Contains(response, x => x.Id == 1 && x.TemperatureF == 10 && x.Summary == "Weather 1");

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

        var result = await HttpClientToUse.SendAsync(request);

        var response = await result.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>() ?? throw new Exception("Can't deserialize result");

        Assert.Equal("application/x-www-form-urlencoded", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Equal("10=Test10&20=Test20", await request.Content!.ReadAsStringAsync());
        Assert.Single(response);
        Assert.Contains(response, x => x.Id == 1 && x.TemperatureF == 10 && x.Summary == "Weather 1");

        VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && 
                                            req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && 
                                            req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value") &&
                                            req.Headers.Any(t => t.Key == "Header2" && t.Value.First() == "Header2Value"));
    }

}

