using LibraryCore.IntegrationTests.Fixtures;
using System.Net;

namespace LibraryCore.IntegrationTests;

public class AspNetAttributeIntegrationTest : IClassFixture<WebApplicationFactoryFixture>
{
    public AspNetAttributeIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture)
    {
        WebApplicationFactoryFixture = webApplicationFactoryFixture;
    }

    private WebApplicationFactoryFixture WebApplicationFactoryFixture { get; }

    [Fact]
    public async Task HttpGetOptionHeadAttribute()
    {
        var valuesToTest = new List<KeyValuePair<HttpMethod, bool>>
            {
                new KeyValuePair<HttpMethod, bool>(HttpMethod.Put, false),
                new KeyValuePair<HttpMethod, bool>(HttpMethod.Post, false),
                new KeyValuePair<HttpMethod, bool>(HttpMethod.Delete, false),
                new KeyValuePair<HttpMethod, bool>(HttpMethod.Patch, false),
                new KeyValuePair<HttpMethod, bool>(HttpMethod.Trace, false),
                new KeyValuePair<HttpMethod, bool>(HttpMethod.Get, true),
                new KeyValuePair<HttpMethod, bool>(HttpMethod.Options, true),
                new KeyValuePair<HttpMethod, bool>(HttpMethod.Head, true)
            };

        foreach (var valueToTest in valuesToTest)
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new HttpRequestMessage(valueToTest.Key, "Simple/AspNetHttpMethodTest"));

            Assert.Equal(valueToTest.Value, response.IsSuccessStatusCode);
            Assert.Equal(valueToTest.Value ? HttpStatusCode.OK : HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }
    }
}
