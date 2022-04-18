using LibraryCore.IntegrationTests.Fixtures;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryCore.IntegrationTests;

public class SqlDataProviderIntegrationTest : IClassFixture<SqlServerTestFixture>
{
    public SqlDataProviderIntegrationTest(SqlServerTestFixture sqlServerTestFixture)
    {
        SqlServerTestFixture = sqlServerTestFixture;
    }

    private SqlServerTestFixture SqlServerTestFixture { get; }
#if DEBUG
    private const string SkipReason = "Don't want to run database integration test locally for now.";
#else
    private const string SkipReason = "";
#endif

    #region Framework

    private static async Task<int> InsertRecordIntoState(SqlServerTestFixture sqlServerTestFixture, string stateDescription, Guid testId)
    {
        await using var dataProvider = sqlServerTestFixture.CreateDataProvider();

        return await dataProvider.ExecuteNonQueryAsync("dbo.CreateState",
                                                        CommandType.StoredProcedure,
                                                        new[]
                                                             {
                                                                new SqlParameter("@TestId", testId),
                                                                new SqlParameter("@Description", stateDescription)
                                                             });
    }

#endregion

    [Fact(Skip = SkipReason)]
    public async Task CanConnectToDatabase()
    {
        await using var dataProvider = SqlServerTestFixture.CreateDataProvider();

        Assert.True(await dataProvider.CanConnectToDatabaseAsync());
    }

    [Fact(Skip = SkipReason)]
    public async Task DatatableTest()
    {

        await using var dataProvider = SqlServerTestFixture.CreateDataProvider();

        var testId = Guid.NewGuid();

        Assert.Equal(1, await InsertRecordIntoState(SqlServerTestFixture, nameof(DatatableTest), testId));

        var dataTable = await dataProvider.DataTableAsync("select * from states where TestId = @TestId", CommandType.Text, new[]
        {
            new SqlParameter("@TestId", testId)
        });

        Assert.Equal(1, dataTable.Rows.Count);
        Assert.Equal(nameof(DatatableTest), dataTable.Rows[0]["Description"]);
    }

    [Fact(Skip = SkipReason)]
    public async Task DataSetTest()
    {
        await using var dataProvider = SqlServerTestFixture.CreateDataProvider();

        var testId = Guid.NewGuid();

        Assert.Equal(1, await InsertRecordIntoState(SqlServerTestFixture, nameof(DataSetTest), testId));

        var dataSet = await dataProvider.DataSetAsync("select * from states where TestId = @TestId", CommandType.Text, new[]
        {
            new SqlParameter("@TestId", testId)
        });

        Assert.Equal(1, dataSet.Tables[0].Rows.Count);
        Assert.Equal(nameof(DataSetTest), dataSet.Tables[0].Rows[0]["Description"]);
    }

    [Fact(Skip = SkipReason)]
    public async Task DataReaderTest()
    {
        await using var dataProvider = SqlServerTestFixture.CreateDataProvider();

        var testId = Guid.NewGuid();

        var readTableParmeters = new[]
        {
            new SqlParameter("@TestId", testId)
        };

        Assert.Equal(1, await InsertRecordIntoState(SqlServerTestFixture, nameof(DataReaderTest), testId));

        //data reader
        using var reader = await dataProvider.DataReaderAsync("select * from states where TestId = @TestId", CommandType.Text, CommandBehavior.SingleRow, readTableParmeters);

        await reader.ReadAsync();

        Assert.Equal(nameof(DataReaderTest), reader["Description"]);
        await reader.CloseAsync();
    }

    [Fact(Skip = SkipReason)]
    public async Task XmlQueryTypes()
    {
        await using var dataProvider = SqlServerTestFixture.CreateDataProvider();

        var testId = Guid.NewGuid();

        var readTableParmeters = new[]
        {
            new SqlParameter("@TestId", testId)
        };

        Assert.Equal(1, await InsertRecordIntoState(SqlServerTestFixture, nameof(XmlQueryTypes), testId));

        //XmlDataAsync will call XmlReaderAsync
        var result = await dataProvider.XmlDataAsync("select * from states where TestId = @TestId for xml path, root('root')", CommandType.Text, readTableParmeters);

        Assert.Single(result.Elements("row"));

        var rowElement = result.Element("row") ?? throw new Exception("No Row Element");

        Assert.Equal(3, rowElement.Elements().Count());
        Assert.Equal(testId, Guid.Parse(rowElement.Element("TestId")!.Value));
        Assert.Equal(nameof(XmlQueryTypes), rowElement.Element("Description")!.Value);
    }

    [Fact(Skip = SkipReason)]
    public async Task ScalarUnTypedTests()
    {
        await using var dataProvider = SqlServerTestFixture.CreateDataProvider();

        var testId = Guid.NewGuid();

        Assert.Equal(1, await InsertRecordIntoState(SqlServerTestFixture, nameof(ScalarUnTypedTests), testId));

        var descriptionFound = await dataProvider.ScalarAsync("select Description from states where TestId = @TestId", CommandType.Text, new[]
        {
            new SqlParameter("@TestId", testId)
        });

        Assert.Equal(nameof(ScalarUnTypedTests), descriptionFound?.ToString());
    }

    [Fact(Skip = SkipReason)]
    public async Task ScalarTypedTests()
    {
        await using var dataProvider = SqlServerTestFixture.CreateDataProvider();

        var testId = Guid.NewGuid();

        Assert.Equal(1, await InsertRecordIntoState(SqlServerTestFixture, nameof(ScalarTypedTests), testId));

        var descriptionFound = await dataProvider.ScalarAsync<string>("select Description from states where TestId = @TestId", CommandType.Text, new[]
        {
            new SqlParameter("@TestId", testId)
        });

        Assert.Equal(nameof(ScalarTypedTests), descriptionFound?.ToString());
    }

    [Fact(Skip = SkipReason)]
    public async Task BulkInsert()
    {
        await using var dataProvider = SqlServerTestFixture.CreateDataProvider();

        var testId = Guid.NewGuid();

        var readTableParmeters = new[]
        {
            new SqlParameter("@TestId", testId)
        };

        var dtToLoad = new DataTable("States");

        var testIdColumn = new DataColumn("TestId", typeof(Guid));
        var stateIdColumn = new DataColumn("StateId", typeof(int))
        {
            AutoIncrement = true
        };
        var descriptionColumn = new DataColumn("Description", typeof(string));

        dtToLoad.Columns.Add(testIdColumn);
        dtToLoad.Columns.Add(stateIdColumn);
        dtToLoad.Columns.Add(descriptionColumn);

        for (int i = 0; i < 2; i++)
        {
            var rowToAdd = dtToLoad.NewRow();

            rowToAdd["TestId"] = testId;
            rowToAdd["Description"] = $"BulkInsert_{i}";

            dtToLoad.Rows.Add(rowToAdd);
        }

        await dataProvider.BulkInsertAsync("dbo", dtToLoad, SqlBulkCopyOptions.Default, 10);

        var results = await dataProvider.DataTableAsync("select * from states where TestId = @TestId", CommandType.Text, readTableParmeters);

        Assert.Equal(2, results.Rows.Count);
        Assert.Equal("BulkInsert_0", results.Rows[0]["Description"]);
        Assert.Equal("BulkInsert_1", results.Rows[1]["Description"]);
    }

}

