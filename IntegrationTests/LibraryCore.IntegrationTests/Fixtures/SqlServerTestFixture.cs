using LibraryCore.Core.DataProviders;
using LibraryCore.IntegrationTests.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LibraryCore.IntegrationTests.Fixtures;

public class SqlServerTestFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = null!;

    internal SqlDataProvider CreateDataProvider() => new(ConnectionString);

    public async Task InitializeAsync()
    {
        //so we can debug it in visual studio. Complete hack
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Db_ConnectionString")))
        {
            Console.WriteLine("No DbConnection String Found In Env Variables. Setting Connection String In " + nameof(SqlServerTestFixture));
            Environment.SetEnvironmentVariable("Db_ConnectionString", "Data Source=localhost;Initial Catalog=IntegrationTest;User Id=sa;Password=Pass@word;trustServerCertificate=true");
        } 
         
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