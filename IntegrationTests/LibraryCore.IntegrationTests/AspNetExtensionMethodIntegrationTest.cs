using LibraryCore.IntegrationTests.Fixtures;
using System.Net.Http.Json;

namespace LibraryCore.IntegrationTests;

public class AspNetExtensionMethodIntegrationTest : IClassFixture<WebApplicationFactoryFixture>
{
    public AspNetExtensionMethodIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture)
    {
        WebApplicationFactoryFixture = webApplicationFactoryFixture;
    }

    private WebApplicationFactoryFixture WebApplicationFactoryFixture { get; }

    [Fact]
    public async Task IsAjaxCall()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Simple/IsAjaxCall");

        request.Headers.Add("X-Requested-With", "XMLHttpRequest");

        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(request);

        Assert.True(await response.Content.ReadFromJsonAsync<bool>());
    }

    [Fact]
    public async Task IsNotAjaxCall()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.GetAsync("Simple/IsAjaxCall");

        Assert.False(await response.Content.ReadFromJsonAsync<bool>());
    }

}
