﻿using LibraryCore.Core.ExtensionMethods;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;

namespace LibraryCore.Core.DataProviders;

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
            await ConnSql.DisposeAsync();
            Disposed = true;
        }
    }

    #endregion

}