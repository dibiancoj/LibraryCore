using LibraryCore.Healthcare.Epic.Authentication;
using Moq.Protected;
using System.Text;
using System.Text.Json;

namespace LibraryCore.Tests.Healthcare.Epic.Authentication;

public class ClientCredentialsAuthenticationTest
{

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

        mockHttpHandler.Protected().Verify(
            nameof(HttpClient.SendAsync),
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(msg => CheckBodyExpression(msg)),
            ItExpr.IsAny<CancellationToken>());
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

}
