using LibraryCore.Core.DataProviders;
using LibraryCore.IntegrationTests.Database;
using Microsoft.EntityFrameworkCore;

namespace LibraryCore.IntegrationTests.Fixtures;

public class SqlServerTestFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = null!;

    internal SqlDataProvider CreateDataProvider() => new(ConnectionString);

    public async Task InitializeAsync()
    {
        using var dbContext = new IntegrationTestDbContext();
        await dbContext.Database.MigrateAsync();
        
        ConnectionString = dbContext.Database.GetConnectionString() ?? throw new Exception("Null Connection String)");
    }

    public async Task DisposeAsync()
    {
        using var dbContext = new IntegrationTestDbContext();

        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE States;");
        //await dbContext.Database.EnsureDeletedAsync();
    }
}