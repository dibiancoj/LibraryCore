﻿using LibraryCore.ApiClient;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static LibraryCore.ApiClient.ContentTypeLookup;
using static LibraryCore.Tests.ApiClient.HttpRequestSetup;

namespace LibraryCore.Tests.ApiClient;

public class FluentRequestTest
{
    public FluentRequestTest()
    {
        HttpRequestMockSetup = new HttpRequestSetup();
    }

    private HttpRequestSetup HttpRequestMockSetup { get; }

    #region Unit Tests

    [Fact]
    public void AddQueryString()
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                .AddQueryString("id", "4");

        Assert.Equal("https://test.api/WeatherForecast?id=4", request.Message.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public void AddQueryStringToRelativePath()
    {
        var request = new FluentRequest(HttpMethod.Get, "WeatherForecast")
                                .AddQueryString("id", "4");

        Assert.Equal("WeatherForecast?id=4", request.Message.RequestUri!.ToString());
    }

    [Fact]
    public void AddMultipleQueryString()
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                .AddQueryString("id", "4")
                                .AddQueryString("id2", "5");

        Assert.Equal("https://test.api/WeatherForecast?id=4&id2=5", request.Message.RequestUri!.AbsoluteUri);
    }

    [InlineData("https://test.api/WeatherForecast/Usa/NewYork?id=10", "NewYork")]
    [InlineData("https://test.api/WeatherForecast/Usa/NewYork/NJ?id=10", "NewYork", "NJ")]
    [InlineData("https://test.api/WeatherForecast/Usa/New%20York/NJ?id=10", "New York", "NJ")] //make sure its encoded
    [Theory]
    public void AddPath(string expectedResult, params string[] pathsToAdd)
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast/Usa?id=10");

        foreach (var pathToAdd in pathsToAdd)
        {
            request = request.AddUrlPath(pathToAdd);
        }

        Assert.Equal(expectedResult, request.Message.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public void AddPaths()
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast/Usa?id=10")
                            .AddUrlPaths("NewYork", "New Jersey");

        Assert.Equal("https://test.api/WeatherForecast/Usa/NewYork/New%20Jersey?id=10", request.Message.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public void AddMultipleQueryStringsAtOnce()
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                .AddQueryStrings(new Dictionary<string, string?>
                                {
                                    { "id", "4" },
                                    { "id2", "5" }
                                });

        Assert.Equal("https://test.api/WeatherForecast?id=4&id2=5", request.Message.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public void AddQueryStringWhenUrlHasQueryString()
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast?id100=100")
                                .AddQueryString("id", "4")
                                .AddQueryString("id2", "5");

        Assert.Equal("https://test.api/WeatherForecast?id100=100&id=4&id2=5", request.Message.RequestUri!.AbsoluteUri);
    }

    [InlineData(AcceptTypeEnum.Json, "application/json")]
    [InlineData(AcceptTypeEnum.TextHtml, "text/html")]
    [Theory]
    public void AddAcceptType(AcceptTypeEnum acceptTypeToTest, string expectedValue)
    {
        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                .AddAcceptType(acceptTypeToTest);

        Assert.Equal(expectedValue, request.Message.Headers.Accept.ToString());
    }

    [Fact]
    public async Task JsonRequestAndResponse()
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
                                                .AddJsonBody(jsonParameters)
                                                .Message;

        var response = await HttpRequestMockSetup.HttpClientToUse.SendAsync(request, TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal("application/json", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Equal(JsonSerializer.Serialize(jsonParameters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var singleResult = Assert.Single(result);
        Assert.True(singleResult.Id == 1 && singleResult.TemperatureF == 10 && singleResult.Summary == "Weather 1");

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));
    }

    [Fact]
    public async Task JsonRequestWithBaseUrlAndResponse()
    {
        HttpRequestMockSetup.HttpClientToUse.BaseAddress = new Uri("https://test.api/WeatherForecast");

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

        var request = new FluentRequest(HttpMethod.Get)
                                         .AddHeader("Header1", "Header1Value")
                                         .AddJsonBody(jsonParameters)
                                         .Message;

        var request2 = new FluentRequest(HttpMethod.Get)
                                       .AddHeader("Header1", "Header1Value")
                                       .AddJsonBody(jsonParameters);

        var response = await HttpRequestMockSetup.HttpClientToUse.SendAsync(request, TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal("application/json", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Equal(JsonSerializer.Serialize(jsonParameters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var singleResult = Assert.Single(result);
        Assert.True(singleResult.Id == 1 && singleResult.TemperatureF == 10 && singleResult.Summary == "Weather 1");

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));
    }

    [Fact]
    public async Task JsonRequestWithBaseUrlAndAppendWithJsonResponse()
    {
        HttpRequestMockSetup.HttpClientToUse.BaseAddress = new Uri("https://test.api/");

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

        var request = new FluentRequest(HttpMethod.Get, "WeatherForecast")
                                         .AddHeader("Header1", "Header1Value")
                                         .AddJsonBody(jsonParameters)
                                         .Message;

        var response = await HttpRequestMockSetup.HttpClientToUse.SendAsync(request, TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal("application/json", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Equal(JsonSerializer.Serialize(jsonParameters, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var singleResult = Assert.Single(result);
        Assert.True(singleResult.Id == 1 && singleResult.TemperatureF == 10 && singleResult.Summary == "Weather 1");

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value"));
    }

    [Fact]
    public async Task FormsEncodedRequestAndResponse()
    {
        var mockResponse = CreateJsonMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new(1, 10, "Weather 1")
            });

        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get &&
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
                                                .Message;

        var response = await HttpRequestMockSetup.HttpClientToUse.SendAsync(request, TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal("application/x-www-form-urlencoded", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Equal("10=Test10&20=Test20", await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var singleResult = Assert.Single(result);
        Assert.True(singleResult.Id == 1 && singleResult.TemperatureF == 10 && singleResult.Summary == "Weather 1");

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get &&
                                            req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri &&
                                            req.Headers.Any(t => t.Key == "Header1" && t.Value.First() == "Header1Value") &&
                                            req.Headers.Any(t => t.Key == "Header2" && t.Value.First() == "Header2Value"));
    }

    [Fact]
    public async Task FileStreamByteArrayRequestAndResponse()
    {
        var mockResponse = CreateJsonMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new(1, 10, "Weather 1")
            });

        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);

        var byteArray = new byte[] { 1, 2, 3 };

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddFileStreamBody("formFiles", new KeyValuePair<string, byte[]>("test.jpg", byteArray))
                                                .Message;

        var response = await HttpRequestMockSetup.HttpClientToUse.SendAsync(request, TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal("multipart/form-data", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Contains("--Upload--", await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var singleResult = Assert.Single(result);
        Assert.True(singleResult.Id == 1 && singleResult.TemperatureF == 10 && singleResult.Summary == "Weather 1");

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);
    }

    [Fact]
    public async Task FileStreamRequestAndResponse()
    {
        var mockResponse = CreateJsonMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new(1, 10, "Weather 1")
            });

        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);

        using var byteArrayStream = new MemoryStream([1, 2, 3]);

        var request = new FluentRequest(HttpMethod.Get, "https://test.api/WeatherForecast")
                                                .AddFileStreamBody("formFiles", new KeyValuePair<string, Stream>("test.jpg", byteArrayStream))
                                                .Message;

        var response = await HttpRequestMockSetup.HttpClientToUse.SendAsync(request, TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal("multipart/form-data", request.Content?.Headers?.ContentType?.MediaType);
        Assert.Contains("--Upload--", await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var singleResult = Assert.Single(result);
        Assert.True(singleResult.Id == 1 && singleResult.TemperatureF == 10 && singleResult.Summary == "Weather 1");

        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri);
    }

    [Fact]
    public async Task BasicAuthentication()
    {
        HttpRequestMockSetup.HttpClientToUse.BaseAddress = new Uri("https://test.api/");

        var mockResponse = CreateJsonMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new(1, 10, "Weather 1")
            });

        const string expectedAuthValue = "Basic cm9vdDpwYXNzQHdvcmQx";

        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Authorization" && t.Value.First() == expectedAuthValue));

        var request = new FluentRequest(HttpMethod.Get, "WeatherForecast")
                                         .AddBasicAuthentication("root", "pass@word1");

        var response = await HttpRequestMockSetup.HttpClientToUse.SendAsync(request, TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");


        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Authorization" && t.Value.First() == expectedAuthValue));
    }

    [InlineData("Token")]
    [InlineData("Bearer")]
    [Theory]
    public async Task BearerAuthentication(string scheme)
    {
        HttpRequestMockSetup.HttpClientToUse.BaseAddress = new Uri("https://test.api/");

        var mockResponse = CreateJsonMockResponse(HttpStatusCode.OK, new List<WeatherForecast>
            {
                new(1, 10, "Weather 1")
            });

        string expectedAuthValue = $"{scheme} abcdefg";

        HttpRequestMockSetup.MockHttpRequest(mockResponse, req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Authorization" && t.Value.First() == expectedAuthValue));

        var request = new FluentRequest(HttpMethod.Get, "WeatherForecast")
                                         .AddAuthenticationHeader(scheme, "abcdefg");

        var response = await HttpRequestMockSetup.HttpClientToUse.SendAsync(request, TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                        .Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");


        HttpRequestMockSetup.VerifyAndThrow(Times.Once(), req => req.Method == HttpMethod.Get && req.RequestUri!.AbsoluteUri == new Uri("https://test.api/WeatherForecast").AbsoluteUri && req.Headers.Any(t => t.Key == "Authorization" && t.Value.First() == expectedAuthValue));
    }

    #endregion

}

