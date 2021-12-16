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
        public async Task HeaderTest()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/HeaderTest")
                                                                                            .AddHeader("MyHeader", "My Value")
                                                                                            .ToMessage());

            var result = await response.EnsureSuccessStatusCode()
                                    .Content.ReadAsStringAsync();

            Assert.Equal("My Value Result", result);
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

        [Fact]
        public async Task SimpleJsonPayloadWithJsonRequestParameters()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleJsonPayloadWithJsonParameters")
                                                                                            .AddJsonBody(new
                                                                                            {
                                                                                                Id = 5,
                                                                                                Text = "5"
                                                                                            })
                                                                                            .ToMessage());

            var result = await response.EnsureSuccessStatusCode()
                                    .Content.ReadFromJsonAsync<ResultModel>() ?? throw new Exception("Can't deserialize result");

            Assert.Equal(6, result.Id);
            Assert.Equal("5_Result", result.Text);
        }
    }
}
