using Microsoft.AspNetCore.Mvc.Testing;

namespace LibraryCore.IntegrationTests.Kafka.Fixtures;

/// <summary>
/// This is an xunit fixture to contain a singleton for all integration tests within a class.
/// </summary>
public class WebApplicationFactoryFixture : IDisposable
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

#if DEBUG
    public const string SkipReason = "Don't want to run Kafka tests locally";
#else
    public const string SkipReason = "";
#endif

    public WebApplicationFactory<Program> ApplicationFactory { get; }
    public HttpClient HttpClientToUse { get; }
    private bool Disposed { get; set; }

    #region Dispose Method

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!this.Disposed)
        {
            if (disposing)
            {
                ApplicationFactory.Dispose();
                HttpClientToUse.Dispose();

            }
        }
        Disposed = true;
    }

    #endregion

}