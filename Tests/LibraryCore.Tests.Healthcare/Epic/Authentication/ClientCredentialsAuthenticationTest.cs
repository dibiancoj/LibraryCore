using LibraryCore.Healthcare.Epic.Authentication;
using LibraryCore.Shared;
using Moq.Protected;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Tests.Healthcare.Epic.Authentication;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(EpicClientCredentialsAuthorizationToken))]
internal partial class JsonContext : JsonSerializerContext
{
}

public class ClientCredentialsAuthenticationTest
{
    internal const string PrivateKeyTester = """
                                -----BEGIN PRIVATE KEY-----
                                MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCsswIa2WMvORRU
                                0Tdmjrguo3qNmH8gOc6Jq82clI7GHzztKcy738//X6PNmBRycelXC+tDVD4AaXZ1
                                ztuGAdMp0T5rMG5+4cR+RRCHxRkaszR2WLUW64O/IMKkPKsFJni/bTey4H8zDPsK
                                1IKVwSBkuRB0EF24CTsEeCkVmzBpE8Hcg3qNOr6kWYyz8fLWkbTbmTeWTahRG2uc
                                IES8fqdHAUsMuhujWtJ/U/tRW9uBdJtxbJKKt/ksfUQZarCqFEGSWF5zLTemaeat
                                vsMjBzLke5xOnf9vz0pe5fOkAHRYmxBDFVEqJIzPkINna6rL0/k7yHgGUWPxxCHi
                                4RAfk/zXAgMBAAECggEABWi4ligHkpZnwUhUQzgRI2onZSvzlLTQIpZo+Dsxmu77
                                ijTBntjMZk6T43/x6VZmspKYVHbdAkegrYvvFZa/tb2XW5AcGzPtuEQS183KktWD
                                Z87bd9OklWXsnWxGXFZEzeoZJoHK5MnGoHjg0NlIWcnvbqiHPEO9v3lsAACTrEF5
                                qFiYxEKj3il4B/HiB7jDG9JpvZ3sjMM2j7uIp+V/K2HOFf4GP4yJNoM22jrwvrc9
                                8qUpL163tJTYLiM1wqFOkm+DAWfhWOXahsZ+MK9n8CBsFbYZt2VSFrntkfq8N8fH
                                poQ3/sjmz42RMDxlD4Y+aWl064oHfE3kPLrBjzEtEQKBgQDTWiAnhzzgoGVmhmB+
                                Z236U6OG/Y5+EqtqcIV0Yfz2x/RTqV2iShd7CUgY+ceSyQUvTt7QFbKM+WDYfh03
                                g2ZZSgceJuQkY3g8Nr/lqNvDis5P9XkK76JoLErlcXRdfTA9HSkTHngdr17/fcvX
                                dtN1Aa6z1kB5MQXsv4fTt99nhQKBgQDRLowXoordADY5dRUoaCPKlpHOMTD32zF7
                                Rh2iR50pB0j6vTnbLdjn1ZvNYECFCupkf/PMXkB8qpp0YW1pyhcZyQN/igi2CxZ8
                                vYptK1EFe7iUVYvK00XkGqh8351bzV3CUOytaphdHCfHM1d+s5+UJn6+qMF4TipP
                                as/DWqSrqwKBgHJzDpB5MmUtVluzjtNEbRCudBIogh4RPBPyhMImQdbdJpibUWIN
                                nzN4LjugHwuEobMhkZS6+ypN7LyvZmDmXekiJkKUqtxtpPFucHIMA52bL+eqtPZQ
                                F/EzcwdWbb4miZW+kbyRtlfU3Kub0mOGLDHjTFiEP/ugqWWvvfzPSEIFAoGAIRYZ
                                totRJ4+nBAOO6ys9rgeO3GxPcITNGNCIU2i8ZQbv9ikEA7UGv/S7hgqZafHGDUtn
                                xoWJ8a4SbCIU6ky6xqtS0FOG+TRbxDM+t/HICPP+yAeX6ubsAR5uVAISo7iiAeBG
                                708S9ndDm3B1gGbs8MP9/C45G8xRSOp+HkoyDa0CgYEAhXGYj0JvE6y+GTx0xhi0
                                vf4b6MF9kiF9rYT3/88Ki1Wty4Kr2DHh2AT5dC8H31xq8jZF1YsTiKJwokPCCjyZ
                                bU8JyNy7JhFqSwOftFYY0Mms7bZw74z8y46QLXWEo/oqCtCKm+x2tg+xzpTRxoia
                                G7vS5dSB0W+KFEG9RPbtgnM=
                                -----END PRIVATE KEY-----
                                """;

    #region Http Call To Retrieve Token

    [Fact]
    public async Task ClientCredentialsTokenAsync()
    {
        const string clientAssertion = "abc";
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpHandler.Object);

        var mockResponse = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,

            Content = new StringContent(JsonSerializer.Serialize(new EpicClientCredentialsAuthorizationToken("Bearer", "Abcdef", "abc def", 90)), Encoding.UTF8, "application/json")
        };

        mockHttpHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(msg => CheckBodyExpression(msg)),
                ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(mockResponse);

        var result = await ClientCredentialsAuthentication.TokenAsync(httpClient, "https://mytokenendpoint", clientAssertion);

        mockHttpHandler
            .Protected()
            .Verify(
                nameof(HttpClient.SendAsync),
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg => CheckBodyExpression(msg)),
                ItExpr.IsAny<CancellationToken>());

        Assert.Equal(2, result.Scopes.Count);
        Assert.Contains(result.Scopes, x => x == "abc");
        Assert.Contains(result.Scopes, x => x == "def");
    }

    [Trait(ErrorMessages.AotUnitTestTraitName, ErrorMessages.AotUnitTestTraitValue)]
    [Fact]
    public async Task ClientCredentialsTokenAsync_Aot()
    {
        const string clientAssertion = "abc";
        var mockHttpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpHandler.Object);

        var mockResponse = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,

            Content = new StringContent(JsonSerializer.Serialize(new EpicClientCredentialsAuthorizationToken("Bearer", "Abcdef", "abc def", 90)), Encoding.UTF8, "application/json")
        };

        mockHttpHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(msg => CheckBodyExpression(msg)),
                ItExpr.IsAny<CancellationToken>())
             .ReturnsAsync(mockResponse);

        var result = await ClientCredentialsAuthentication.TokenAsync(httpClient, "https://mytokenendpoint", clientAssertion, JsonContext.Default.EpicClientCredentialsAuthorizationToken);

        mockHttpHandler
            .Protected()
            .Verify(
                nameof(HttpClient.SendAsync),
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(msg => CheckBodyExpression(msg)),
                ItExpr.IsAny<CancellationToken>());

        Assert.Equal(2, result.Scopes.Count);
        Assert.Contains(result.Scopes, x => x == "abc");
        Assert.Contains(result.Scopes, x => x == "def");
    }

    private static bool CheckBodyExpression(HttpRequestMessage msg)
    {
        const string expectedBody = "grant_type=client_credentials&client_assertion_type=urn%3Aietf%3Aparams%3Aoauth%3Aclient-assertion-type%3Ajwt-bearer&client_assertion=abc";

        //trying to avoid using async within a sync method. So putting this in a stream to read the body
        var reader = new StreamReader(msg.Content!.ReadAsStream());

        var bodyContent = reader.ReadToEnd();

        //rewind for any future reads
        msg.Content!.ReadAsStream().Position = 0;

        return msg.Method == HttpMethod.Post &&
               msg.RequestUri == new Uri("https://mytokenendpoint") &&
               bodyContent.Equals(expectedBody);
    }

    #endregion

    #region Jwt Client Assertion Creator

    [Fact]
    public void ClientAssertionCreateTest()
    {
        //we ran into "Safe handle has been closed" when you run it multiple times. So we are putting this in a for loop to verify that works in current code
        for (int i = 0; i < 10; i++)
        {
            var clientId = Guid.NewGuid().ToString();

            var generatedToken = ClientCredentialsAuthentication.CreateEpicClientAssertionJwtToken(PrivateKeyTester, clientId, "http://token.com");

            var handler = new JwtSecurityTokenHandler();

            var readJwt = handler.ReadJwtToken(generatedToken);

            Assert.Contains("http://token.com", readJwt.Audiences);
            Assert.Equal(clientId, readJwt.Issuer);
            Assert.Contains(readJwt.Claims, x => x.Type == JwtRegisteredClaimNames.Sub && x.Value == clientId);
            Assert.Contains(readJwt.Claims, x => x.Type == JwtRegisteredClaimNames.Jti && Guid.TryParse(x.Value, out _)); //just make sure its a guid
        }
    }

    #endregion

}
