using Microsoft.AspNetCore.Mvc.Testing;

namespace LibraryCore.IntegrationTests.Fixtures
{
    public class WebApplicationFactoryFixture
    {
        public WebApplicationFactoryFixture()
        {
            ApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
              {
                  // ... Configure test services
              });

            HttpClientToUse = ApplicationFactory.CreateClient();
        }

        public WebApplicationFactory<Program> ApplicationFactory { get; }
        public HttpClient HttpClientToUse { get; }
    }
}
