using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.HttpRequestCore;
using LibraryCore.Tests.Core.GlobalMocks;
using System.Net;
using static LibraryCore.Core.ExtensionMethods.HttpClientExtensionMethods;
using static LibraryCore.Tests.Core.GlobalMocks.HttpRequestSetup;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class HttpClientExtensionMethodTest
{
    public HttpClientExtensionMethodTest()
    {
        HttpRequestMockSetup = new HttpRequestSetup();
    }

    private HttpRequestSetup HttpRequestMockSetup { get; }

    #region Framework

    public class XmlRoot
    {
        public int Id { get; set; }
    }

    #endregion

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task HttpClientSendRequestToJsonTest(bool includeCancelToken)
    {
        var mockResponse = CreateJsonMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new WeatherForecast(1, 10, "Weather 1")
            });

        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));

        var jsonParameters = new
        {
            Id = 10,
            Name = "Test"
        };

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddHeader("Header1", "Header1Value")
                                                .AddJsonBody(jsonParameters);

        var result = await HttpRequestMockSetup.HttpClientToUse.SendRequestToJsonAsync<IEnumerable<WeatherForecast>>(request, includeCancelToken ? new CancellationToken() : default) ?? throw new Exception("Can't deserialize result");

        Assert.Single(result);
        Assert.Contains(result, x => x.Id == 1 && x.TemperatureF == 10 && x.Summary == "Weather 1");

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task HttpClientSendRequestToXmlTest(bool includeCancelToken)
    {
        var mockResponse = CreateXmlMockResponse(HttpStatusCode.OK, new HttpContentExtensionMethodTest.TestModel
        {
            Id = 24,
            Name = "Test"
        });

        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));

        var jsonParameters = new
        {
            Id = 10,
            Name = "Test"
        };

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddHeader("Header1", "Header1Value")
                                                .AddJsonBody(jsonParameters);

        var result = await HttpRequestMockSetup.HttpClientToUse.SendRequestToXmlAsync<HttpContentExtensionMethodTest.TestModel>(request, includeCancelToken ? new CancellationToken() : default) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(24, result.Id);
        Assert.Equal("Test", result.Name);

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));
    }

    [Fact]
    public async Task TokenFetch()
    {
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();

        var mockResponse = CreateJsonMockResponse(HttpStatusCode.OK, new Token("my_token_type", "Abcdef", "test_scope", 3600, now));


        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Post &&
                                                           req.RequestUri!.AbsoluteUri == new Uri("https://mygateway/token").AbsoluteUri);


        var result = (await HttpRequestMockSetup.HttpClientToUse.TokenAsync(new Uri("https://mygateway/token"),
                                                                           "MyToken",
                                                                           "MySecret",
                                                                           grantType: "test_credentials",
                                                                           scope: "test_scope"))!;

        Assert.Equal("my_token_type", result.TokenType);
        Assert.Equal("Abcdef", result.AccessToken);
        Assert.Equal("test_scope", result.Scope);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(now).ToLocalTime().AddSeconds(3600), result.ExpiresLocalTime);

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Post &&
                                                                 req.RequestUri!.AbsoluteUri == new Uri("https://mygateway/token").AbsoluteUri);
    }
}
