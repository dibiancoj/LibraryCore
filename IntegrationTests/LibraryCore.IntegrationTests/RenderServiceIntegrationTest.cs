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

        private static string RemoveLineEndings(string text) => text.ReplaceLineEndings(string.Empty);

        private static void AssertIgnoreLineEndings(string expectedValue, string actual) => Assert.Equal(RemoveLineEndings(expectedValue), RemoveLineEndings(actual));

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

            AssertIgnoreLineEndings("<div>Test</div>", result);
        }

        [Fact]
        public async Task RenderServiceWithModel()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.GetAsync("RenderService/WithModel");

            var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            AssertIgnoreLineEndings("\r\n<div aria-key=key1 aria-value=value1>\r\n</div>", result);
        }
    }
}
