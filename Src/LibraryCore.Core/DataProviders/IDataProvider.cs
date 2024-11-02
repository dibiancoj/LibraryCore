using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;

namespace LibraryCore.Core.DataProviders;

public interface IDataProvider : IDisposable, IAsyncDisposable
{
    Task<bool> CanConnectToDatabaseAsync();
    Task CloseConnectionAsync();

    Task<DataSet> DataSetAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null, string dataSetName = "ds");
    Task<DataTable> DataTableAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null, string TableName = "dt");
    Task<int> ExecuteNonQueryAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    Task<DbDataReader> DataReaderAsync(string sqlToRun, CommandType commandTypeToRun, CommandBehavior commandBehaviorToUse, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    Task<XmlReader> XmlReaderAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    Task<XElement> XmlDataAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    Task<object?> ScalarAsync(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    Task<T?> ScalarAsync<T>(string sqlToRun, CommandType commandTypeToRun, IEnumerable<SqlParameter>? queryParameters = null, int? commandTimeOut = null);
    Task BulkInsertAsync(string dataTableSchema, DataTable dataTableToLoad, SqlBulkCopyOptions copyOptions, int batchSize, int? commandTimeOut = null);
}
