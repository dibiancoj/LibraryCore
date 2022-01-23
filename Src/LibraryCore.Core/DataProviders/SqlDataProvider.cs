using LibraryCore.Core.ExtensionMethods;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;

namespace LibraryCore.Core.DataProviders;

[ExcludeFromCodeCoverage(Justification = "Tested In Integration Project. Using this as a 1 off")]
public class SqlDataProvider : IDataProvider, IAsyncDisposable
{

    #region Constructor

    public SqlDataProvider(string connectionString)
    {
        ConnSql = new SqlConnection(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
    }

    #endregion

    #region Properties

    private SqlConnection ConnSql { get; }

    private bool Disposed { get; set; }

    #endregion

    #region Main Public Methods

    public async Task<DataSet> DataSetAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeout = null, string dataSetName = "ds")
    {
        try
        {
            using var dataSetTo = new DataSet(dataSetName);
            using var commandToRun = CreateSqlCommand(sqlToRun, ConnSql, commandTypeToRun, queryParameters, commandTimeout);
            using var dataAdapter = new SqlDataAdapter(commandToRun);

            dataAdapter.Fill(dataSetTo);

            return dataSetTo;
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    public async Task<DataTable> DataTableAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeout = null, string TableName = "dt")
    {
        try
        {
            using var dataTableTo = new DataTable(TableName);
            using var commandToRun = CreateSqlCommand(sqlToRun, ConnSql, commandTypeToRun, queryParameters, commandTimeout);
            using var dataAdapter = new SqlDataAdapter(commandToRun);

            dataAdapter.Fill(dataTableTo);

            return dataTableTo;
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    public async Task<int> ExecuteNonQueryAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null)
    {
        try
        {
            using var commandToRun = await OpenIfNotOpenedAlreadyAsync(CreateSqlCommand(sqlToRun, ConnSql, commandTypeToRun, queryParameters, commandTimeOut));

            return await commandToRun.ExecuteNonQueryAsync();
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    public async Task<DbDataReader> DataReaderAsync(string sqlToRun, CommandType commandTypeToRun, CommandBehavior commandBehaviorToUse, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null)
    {
        try
        {
            using var commandToRun = await OpenIfNotOpenedAlreadyAsync(CreateSqlCommand(sqlToRun, ConnSql, commandTypeToRun, queryParameters, commandTimeOut));

            return await commandToRun.ExecuteReaderAsync(commandBehaviorToUse);
        }
        catch (Exception)
        {
            await CloseConnectionAsync();

            throw;
        }
    }

    public async Task<XmlReader> XmlReaderAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null)
    {
        try
        {
            using var commandToRun = await OpenIfNotOpenedAlreadyAsync(CreateSqlCommand(sqlToRun, ConnSql, commandTypeToRun, queryParameters, commandTimeOut));

            return await commandToRun.ExecuteXmlReaderAsync();
        }
        catch (Exception)
        {
            await CloseConnectionAsync();

            throw;
        }
    }

    public async Task<XElement> XmlDataAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null)
    {
        using var XmlReaderToLoad = await XmlReaderAsync(sqlToRun, commandTypeToRun, queryParameters, commandTimeOut);

        return XElement.Load(XmlReaderToLoad);
    }

    public async Task<object?> ScalarAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null)
    {
        try
        {
            using var commandToRun = await OpenIfNotOpenedAlreadyAsync(CreateSqlCommand(sqlToRun, ConnSql, commandTypeToRun, queryParameters, commandTimeOut));

            return await commandToRun.ExecuteScalarAsync();
        }
        finally
        {
            await CloseConnectionAsync();
        }
    }

    public async Task<T?> ScalarAsync<T>(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null) => (T?)await ScalarAsync(sqlToRun, commandTypeToRun, queryParameters, commandTimeOut);

    public async Task BulkInsertAsync(string dataTableSchema, DataTable dataTableToLoad, SqlBulkCopyOptions copyOptions, int batchSize, int? commandTimeOut = null)
    {
        await ConnSql.OpenAsync();

        SqlTransaction? sqlTransactionToUse = null;

        try
        {
            sqlTransactionToUse = ConnSql.BeginTransaction();

            using var sqlToRunBulk = new SqlBulkCopy(ConnSql, copyOptions, sqlTransactionToUse)
            {
                BatchSize = batchSize,
                DestinationTableName = $"{dataTableSchema}.{dataTableToLoad.TableName}"
            };

            if (commandTimeOut.HasValue)
            {
                sqlToRunBulk.BulkCopyTimeout = commandTimeOut.Value;
            }

            sqlToRunBulk.WriteToServer(dataTableToLoad.CreateDataReader());

            sqlTransactionToUse.Commit();
        }
        catch (Exception)
        {
            sqlTransactionToUse?.Rollback();

            throw;
        }
        finally
        {
            if (sqlTransactionToUse != null)
            {
                await sqlTransactionToUse.DisposeAsync();
            }

            await CloseConnectionAsync();
        }
    }

    #endregion

    #region Public Static Methods

    public static string BuildConnectionString(string serverAndInstanceName, string databaseName, string userId, string password) => $"Server={serverAndInstanceName};Database={databaseName};User Id={userId};Password={password};";
    public static string BuildConnectionString(string serverAndInstanceName, string databaseName) => $"Server={serverAndInstanceName};Database={databaseName};Trusted_Connection=True;";

    #endregion

    #region Supporting Methods

    public async Task<bool> CanConnectToDatabaseAsync()
    {
        await ConnSql.OpenAsync();
        await ConnSql.CloseAsync();

        return true;
    }

    public async Task CloseConnectionAsync() => await ConnSql.CloseAsync();

    #endregion

    #region Private Methods Methods

    private static SqlCommand CreateSqlCommand(string sqlToRun, SqlConnection sqlConnection, CommandType commandType, IEnumerable<SqlParameter>? queryParameters, int? commandTimeOut)
    {
        var commandToRun = new SqlCommand(sqlToRun, sqlConnection)
        {
            CommandType = commandType
        };

        if (queryParameters.AnyWithNullCheck())
        {
            commandToRun.Parameters.AddRange(queryParameters.ToArray());
        }

        if (commandTimeOut.HasValue)
        {
            commandToRun.CommandTimeout = commandTimeOut.Value;
        }

        return commandToRun;
    }

    private static async Task<SqlCommand> OpenIfNotOpenedAlreadyAsync(SqlCommand commandToRun)
    {
        if (commandToRun.Connection.State == ConnectionState.Closed)
        {
            await commandToRun.Connection.OpenAsync();
        }

        return commandToRun;
    }

    #endregion

    #region Dispose Method

    public async ValueTask DisposeAsync()
    {
        if (!Disposed)
        {
            Disposed = true;
            await DisposeAsyncInternal(true);
            GC.SuppressFinalize(this);
        }
    }

    protected virtual async ValueTask DisposeAsyncInternal(bool disposing)
    {
        if (disposing)
        {
            await ConnSql.CloseAsync();
            await ConnSql.DisposeAsync();
            Disposed = true;
        }
    }

    #endregion

}