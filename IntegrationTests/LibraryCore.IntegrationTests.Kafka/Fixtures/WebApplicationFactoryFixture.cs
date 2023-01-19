using Microsoft.AspNetCore.Mvc.Testing;

namespace LibraryCore.IntegrationTests.Kafka.Fixtures;

/// <summary>
/// This is an xunit fixture to contain a singleton for all integration tests within a class.
/// </summary>
public class WebApplicationFactoryFixture : IDisposable
{
    public WebApplicationFactoryFixture()
    {
        //skip building the factory and causing the web project to spin up if we aren't even running this test.
        if (IsUnitTestGoingToRun())
        {
            ApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // ... Configure test services
            });

            HttpClientToUse = ApplicationFactory.CreateClient();
        }
    }

#if DEBUG
    public const string SkipReason = "Don't want to run Kafka tests locally";
#else
    public const string SkipReason = "";
#endif

    public static bool IsUnitTestGoingToRun() => string.IsNullOrEmpty(SkipReason);

    public WebApplicationFactory<Program> ApplicationFactory { get; } = null!;
    public HttpClient HttpClientToUse { get; } = null!;
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
            if (IsUnitTestGoingToRun() && disposing)
            {
                ApplicationFactory.Dispose();
                HttpClientToUse.Dispose();

            }
        }
        Disposed = true;
    }

    #endregion

}