using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public interface IDbReader : IDbCommandExecutor, IAsyncDisposable, IDisposable
    {
        ValueTask<IEnumerable<T>> EnumerableAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class, IDbReadable<T>, new();
        ValueTask<IEnumerable<T>> EnumerableAsync<T>(string query, IDataMapper<T> mapper, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class;
        ValueTask<T?> SingleAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class, IDbReadable<T>, new();
        ValueTask<T?> SingleAsync<T>(string query, IDataMapper<T> mapper, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class;
#if NET5_0_OR_GREATER
        ValueTask<T?> ScalarAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#else
        ValueTask<T> ScalarAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#endif
        ValueTask<IDataReader> StoredProcedureAsync(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#if NET5_0_OR_GREATER
        ValueTask<T?> StoredProcedureAsync<T>(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#else
        ValueTask<T> StoredProcedureAsync<T>(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#endif
    }
}
