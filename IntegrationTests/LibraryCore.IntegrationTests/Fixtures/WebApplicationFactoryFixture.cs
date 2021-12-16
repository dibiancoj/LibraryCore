using Microsoft.AspNetCore.Mvc.Testing;

namespace LibraryCore.IntegrationTests.Fixtures
{
    public class WebApplicationFactoryFixture: IDisposable
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
        private bool Disposed { get; set; }

        #region Dispose Method

        /// <summary>
        /// Disposes My Object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose Overload. Ensures my database connection is closed
        /// </summary>
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
}
