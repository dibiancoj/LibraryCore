using LibraryCore.Core.DataProviders;

namespace LibraryCore.Tests.Core.DataProviders;

public class SqlDataProviderTest
{
    [Fact]
    public void NullConnectionStringInCtor()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlDataProvider(null!));
    }

    [InlineData("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;", "myServerAddress", "myDataBase", "myUsername", "myPassword")] //standard
    [InlineData(@"Server=myServerAddress\myInstance;Database=myDataBase;User Id=myUsername;Password=myPassword;", @"myServerAddress\myInstance", "myDataBase", "myUsername", "myPassword")] //with specific instance
    [InlineData(@"Server=myServerAddress\myInstance,4545;Database=myDataBase;User Id=myUsername;Password=myPassword;", @"myServerAddress\myInstance,4545", "myDataBase", "myUsername", "myPassword")] //with non-standard port
    [Theory]
    public void BuildConnectionStringStandard(string expectedConnectionString, string serverAndInstanceName, string databaseName, string userId, string password)
    {
        Assert.Equal(expectedConnectionString, SqlDataProvider.BuildConnectionString(serverAndInstanceName, databaseName, userId, password));
    }

    [InlineData("Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;", "myServerAddress", "myDataBase")] //standard
    [InlineData(@"Server=myServerAddress\myInstance;Database=myDataBase;Trusted_Connection=True;", @"myServerAddress\myInstance", "myDataBase")] //with specific instance
    [InlineData(@"Server=myServerAddress\myInstance,4545;Database=myDataBase;Trusted_Connection=True;", @"myServerAddress\myInstance,4545", "myDataBase")] //with non-standard port
    [Theory]
    public void BuildConnectionStringWithIntegratedSecurity(string expectedConnectionString, string serverAndInstanceName, string databaseName)
    {
        Assert.Equal(expectedConnectionString, SqlDataProvider.BuildConnectionString(serverAndInstanceName, databaseName));
    }

    [Fact]
    public async Task DisposeAsyncCorrectly()
    {
        //shouldn't throw.
        await using var DataProvider = new SqlDataProvider(SqlDataProvider.BuildConnectionString("localhost,5434", "Test"));
    }

}

