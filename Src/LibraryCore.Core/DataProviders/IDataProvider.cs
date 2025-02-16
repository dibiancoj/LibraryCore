using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;

namespace LibraryCore.Core.DataProviders;

public interface IDataProvider : IDisposable, IAsyncDisposable
{
    public Task<bool> CanConnectToDatabaseAsync();
    public Task CloseConnectionAsync();

    public Task<DataSet> DataSetAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null, string dataSetName = "ds");
    public Task<DataTable> DataTableAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null, string TableName = "dt");
    public Task<int> ExecuteNonQueryAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    public Task<DbDataReader> DataReaderAsync(string sqlToRun, CommandType commandTypeToRun, CommandBehavior commandBehaviorToUse, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    public Task<XmlReader> XmlReaderAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    public Task<XElement> XmlDataAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    public Task<object?> ScalarAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    public Task<T?> ScalarAsync<T>(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    public Task BulkInsertAsync(string dataTableSchema, DataTable dataTableToLoad, SqlBulkCopyOptions copyOptions, int batchSize, int? commandTimeOut = null);
}
