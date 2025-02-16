using LibraryCore.IntegrationTests.Fixtures;
using System.Net;

namespace LibraryCore.IntegrationTests;

public class AspNetAttributeIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture) : IClassFixture<WebApplicationFactoryFixture>
{
    [Fact]
    public async Task HttpGetOptionHeadAttribute()
    {
        var valuesToTest = new List<KeyValuePair<HttpMethod, bool>>
            {
                new(HttpMethod.Put, false),
                new(HttpMethod.Post, false),
                new(HttpMethod.Delete, false),
                new(HttpMethod.Patch, false),
                new(HttpMethod.Trace, false),
                new(HttpMethod.Get, true),
                new(HttpMethod.Options, true),
                new(HttpMethod.Head, true)
            };

        foreach (var valueToTest in valuesToTest)
        {
            var response = await webApplicationFactoryFixture.HttpClientToUse.SendAsync(new HttpRequestMessage(valueToTest.Key, "Simple/AspNetHttpMethodTest"), TestContext.Current.CancellationToken);

            Assert.Equal(valueToTest.Value, response.IsSuccessStatusCode);
            Assert.Equal(valueToTest.Value ? HttpStatusCode.OK : HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }
    }
}
