using LibraryCore.Core.HttpRequestCore;
using LibraryCore.IntegrationTests.Fixtures;
using System.Net.Http.Json;

namespace LibraryCore.IntegrationTests
{
    public class FluentHttpRequestIntegrationTest : IClassFixture<WebApplicationFactoryFixture>
    {
        public WebApplicationFactoryFixture WebApplicationFactoryFixture { get; }

        public record ResultModel(int Id, string Text);

        public FluentHttpRequestIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture)
        {
            WebApplicationFactoryFixture = webApplicationFactoryFixture;
        }

        [Fact]
        public async Task SimpleJsonPayload()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleJsonPayload")
                                                                                            .ToMessage());

            var result = await response.EnsureSuccessStatusCode()
                                    .Content.ReadFromJsonAsync<ResultModel>() ?? throw new Exception("Can't deserialize result");

            Assert.Equal(9999, result.Id);
            Assert.Equal("1111", result.Text);
        }
    }
}
