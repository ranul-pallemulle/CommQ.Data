using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public interface IDbReader : IAsyncDisposable, IDisposable
    {
        ValueTask<IDataReader> RawAsync(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
        ValueTask<IEnumerable<T>> EnumerableAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : IDbReadable<T>, new();
        ValueTask<T> SingleAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : IDbReadable<T>, new();
        ValueTask<T> ScalarAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
    }
}
