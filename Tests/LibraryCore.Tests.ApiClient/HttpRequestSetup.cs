using LibraryCore.XmlSerialization;
using Moq.Protected;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LibraryCore.Tests.ApiClient;

public class HttpRequestSetup
{
    public HttpRequestSetup()
    {
        MockHttpHandler = new Mock<HttpMessageHandler>();
        HttpClientToUse = new HttpClient(MockHttpHandler.Object);
    }

    public Mock<HttpMessageHandler> MockHttpHandler { get; }
    public HttpClient HttpClientToUse { get; }

    public record WeatherForecast(int Id, int TemperatureF, string Summary);

    public static HttpResponseMessage CreateJsonMockResponse<T>(HttpStatusCode httpStatusCode, T modelToExpect)
    {
        return new HttpResponseMessage
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(JsonSerializer.Serialize(modelToExpect))
        };
    }

    public static HttpResponseMessage CreateXmlMockResponse<T>(HttpStatusCode httpStatusCode, T modelToExpect)
    {
        return new HttpResponseMessage
        {
            StatusCode = httpStatusCode,
            Content = new StringContent(XMLSerializationHelper.SerializeObjectToXElement(modelToExpect).ToString(), Encoding.UTF8, "text/xml")
        };
    }

    public void MockHttpRequest(HttpResponseMessage mockResponseToReturn, Expression<Func<HttpRequestMessage, bool>> messageCheck)
    {
        MockHttpHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.Is(messageCheck),
              ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(mockResponseToReturn);
    }

    //private void MockHttpRequest(HttpResponseMessage mockResponseToReturn)
    //{
    //    MockHttpRequest(mockResponseToReturn, x => true);
    //}

    public void VerifyAndThrow(Times times, Expression<Func<HttpRequestMessage, bool>> expressionToVerify)
    {
        MockHttpHandler.Protected().Verify(
             nameof(HttpClient.SendAsync),
             times,
             ItExpr.Is(expressionToVerify),
             ItExpr.IsAny<CancellationToken>());
    }
}
