using LibraryCore.ApiClient;
using LibraryCore.ApiClient.ExtensionMethods;
using System.Net;
using System.Text.Json.Serialization;
using static LibraryCore.ApiClient.ExtensionMethods.HttpClientExtensionMethods;
using static LibraryCore.Tests.ApiClient.HttpRequestSetup;

namespace LibraryCore.Tests.ApiClient;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Token))]
internal partial class ApiJsonContext : JsonSerializerContext
{
}


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
                new(1, 10, "Weather 1")
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

        var result = await HttpRequestMockSetup.HttpClientToUse.SendRequestToJsonAsync<IEnumerable<WeatherForecast>>(request, cancellationToken: includeCancelToken ? new CancellationToken() : default) ?? throw new Exception("Can't deserialize result");

        Assert.Single(result);
        Assert.Contains(result, x => x.Id == 1 && x.TemperatureF == 10 && x.Summary == "Weather 1");

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));
    }

    [Fact]
    public async Task HttpClientSendRequestToJsonWithUnionTypesWithNon200OrNon400Test()
    {
        var mockResponse = CreateJsonMockResponse(HttpStatusCode.InternalServerError, "dummy value");

        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddHeader("Header1", "Header1Value");

        var exceptionRaised = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            _ = await HttpRequestMockSetup.HttpClientToUse.SendRequestToJsonAsync<IEnumerable<WeatherForecast>, Dictionary<string, string>>(request);
        });

        Assert.NotNull(exceptionRaised);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task HttpClientSendRequestToJsonWithUnionTypesTest(bool isSuccessfulApiCall)
    {
        var mockResponse = isSuccessfulApiCall ? CreateJsonMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
                                                    {
                                                        new(1, 10, "Weather 1")
                                                    }) :
                                                    CreateJsonMockResponse(HttpStatusCode.BadRequest, new Dictionary<string, string>
                                                                            {
                                                                                   { "Id", "Is Null"}
                                                                             });

        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddHeader("Header1", "Header1Value");

        var result = await HttpRequestMockSetup.HttpClientToUse.SendRequestToJsonAsync<IEnumerable<WeatherForecast>, Dictionary<string, string>>(request);

        Assert.Equal(isSuccessfulApiCall, result.IsSuccessful);

        if (isSuccessfulApiCall)
        {
            Assert.True(result.TryGetIsSuccessful(out var successModel));
            Assert.Single(successModel);
            Assert.Contains(successModel, x => x.Id == 1 && x.TemperatureF == 10 && x.Summary == "Weather 1");

            Assert.False(result.TryGetIsBadRequest(out var badResultModel));
            Assert.Null(badResultModel);
        }
        else
        {
            Assert.True(result.TryGetIsBadRequest(out var badResultModel));
            Assert.Single(badResultModel);
            Assert.Contains(badResultModel, x => x.Key == "Id" && x.Value == "Is Null");

            Assert.False(result.TryGetIsSuccessful(out var successModelAttempt2));
            Assert.Null(successModelAttempt2);
        }

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);
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
                                                           req.RequestUri!.AbsoluteUri == "https://mygateway/token");


        var result = (await HttpRequestMockSetup.HttpClientToUse.TokenAsync("https://mygateway/token",
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

    [Trait("CompileMode", "Aot")]
    [Fact]
    public async Task TokenFetch_Aot()
    {
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();

        var mockResponse = CreateJsonMockResponse(HttpStatusCode.OK, new Token("my_token_type", "Abcdef", "test_scope", 3600, now));


        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Post &&
                                                           req.RequestUri!.AbsoluteUri == "https://mygateway/token");


        var result = (await HttpRequestMockSetup.HttpClientToUse.TokenAsync("https://mygateway/token",
                                                                           "MyToken",
                                                                           "MySecret",
                                                                           ApiJsonContext.Default.Token,
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

    [Fact]
    public void TokenIsExpired_NotExpired()
    {
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();

        var token = new Token("my_token_type", "Abcdef", "test_scope", 3600, now);

        Assert.False(token.IsExpired());
        Assert.False(token.IsExpired(new TimeSpan(0, 1, 0)));
    }

    [Fact]
    public void TokenIsExpired_IsExpired()
    {
        var now = DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds();

        var token = new Token("my_token_type", "Abcdef", "test_scope", 3600, now);

        Assert.True(token.IsExpired());
        Assert.True(token.IsExpired(new TimeSpan(0, 1, 0)));

        //throw a test in here to ensure if we buffer it more then it shouldn't be expired
        Assert.False(token.IsExpired(new TimeSpan(5, 0, 0, 0)));
    }
}
