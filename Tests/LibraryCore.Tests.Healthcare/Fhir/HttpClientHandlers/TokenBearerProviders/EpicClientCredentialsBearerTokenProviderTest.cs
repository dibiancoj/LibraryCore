using LibraryCore.Healthcare.Epic.Authentication;
using LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders.Implementations;
using LibraryCore.Shared;
using LibraryCore.Tests.Healthcare.Epic.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq.Protected;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Tests.Healthcare.Fhir.HttpClientHandlers.TokenBearerProviders;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(EpicClientCredentialsAuthorizationToken))]
internal partial class JsonContextClientCredentailsTokenProvider : JsonSerializerContext
{
}

public class EpicClientCredentialsBearerTokenProviderTest
{
    [Fact]
    public async Task CachingEpicClientCredsTest()
    {
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpHandler.Object);

        var mockResponse1 = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new EpicClientCredentialsAuthorizationToken("Bearer", "Abcdef", "abc def", 5)), Encoding.UTF8, "application/json")
        };

        var mockResponse2 = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new EpicClientCredentialsAuthorizationToken("Bearer", "zzzzzz", "abc def", 5)), Encoding.UTF8, "application/json")
        };

        mockHttpHandler
          .Protected()
          .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(msg => true),
                ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(mockResponse1)
             .ReturnsAsync(mockResponse2);

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var tokenProvider = new EpicClientCredentialsBearerTokenProvider(memoryCache,
                                                                         httpClient,
                                                                         "http://www.fhir.token.com",
                                                                         ClientCredentialsAuthenticationTest.PrivateKeyTester,
                                                                         "ClientAbc");

        Assert.Equal("Abcdef", await tokenProvider.AccessTokenAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Abcdef", await tokenProvider.AccessTokenAsync(TestContext.Current.CancellationToken));

        await Task.Delay(TimeSpan.FromSeconds(6), TestContext.Current.CancellationToken);

        Assert.Equal("zzzzzz", await tokenProvider.AccessTokenAsync(TestContext.Current.CancellationToken));
        Assert.Equal("zzzzzz", await tokenProvider.AccessTokenAsync(TestContext.Current.CancellationToken));

        mockHttpHandler
         .Protected()
         .Verify<Task<HttpResponseMessage>>(
               "SendAsync",
               Times.Exactly(2),
               ItExpr.Is<HttpRequestMessage>(msg => true),
               ItExpr.IsAny<CancellationToken>());
    }

    [Trait(ErrorMessages.AotUnitTestTraitName, ErrorMessages.AotUnitTestTraitValue)]
    [Fact]
    public async Task CachingEpicClientCredsTest_Aot()
    {
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpHandler.Object);

        var mockResponse1 = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new EpicClientCredentialsAuthorizationToken("Bearer", "Abcdef", "abc def", 5)), Encoding.UTF8, "application/json")
        };

        var mockResponse2 = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new EpicClientCredentialsAuthorizationToken("Bearer", "zzzzzz", "abc def", 5)), Encoding.UTF8, "application/json")
        };

        mockHttpHandler
          .Protected()
          .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(msg => true),
                ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(mockResponse1)
             .ReturnsAsync(mockResponse2);

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var tokenProvider = new EpicClientCredentialsBearerTokenProvider(memoryCache,
                                                                         httpClient,
                                                                         "http://www.fhir.token.com",
                                                                         ClientCredentialsAuthenticationTest.PrivateKeyTester,
                                                                         "ClientAbc",
                                                                         JsonContextClientCredentailsTokenProvider.Default.EpicClientCredentialsAuthorizationToken);

        Assert.Equal("Abcdef", await tokenProvider.AccessTokenAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Abcdef", await tokenProvider.AccessTokenAsync(TestContext.Current.CancellationToken));

        await Task.Delay(TimeSpan.FromSeconds(6), TestContext.Current.CancellationToken);

        Assert.Equal("zzzzzz", await tokenProvider.AccessTokenAsync(TestContext.Current.CancellationToken));
        Assert.Equal("zzzzzz", await tokenProvider.AccessTokenAsync(TestContext.Current.CancellationToken));

        mockHttpHandler
         .Protected()
         .Verify<Task<HttpResponseMessage>>(
               "SendAsync",
               Times.Exactly(2),
               ItExpr.Is<HttpRequestMessage>(msg => true),
               ItExpr.IsAny<CancellationToken>());
    }
}
