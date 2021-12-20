using LibraryCore.IntegrationTests.Fixtures;

namespace LibraryCore.IntegrationTests
{
    public class RenderServiceIntegrationTest : IClassFixture<WebApplicationFactoryFixture>
    {
        public RenderServiceIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture)
        {
            WebApplicationFactoryFixture = webApplicationFactoryFixture;
        }

        private WebApplicationFactoryFixture WebApplicationFactoryFixture { get; }

        [Fact]
        public async Task CantFindView()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.GetAsync("RenderService/CantFindView");

            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task RenderServiceWithNoModel()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.GetAsync("RenderService/NoModel");

            var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            Assert.Equal("\"\\u003Cdiv\\u003ETest\\u003C/div\\u003E\"", result);
        }

        [Fact]
        public async Task RenderServiceWithModel()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.GetAsync("RenderService/WithModel");

            var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            Assert.Equal("\"\\r\\n\\u003Cdiv aria-key=key1 aria-value=value1\\u003E\\r\\n\\u003C/div\\u003E\"", result);
        }
    }
}
