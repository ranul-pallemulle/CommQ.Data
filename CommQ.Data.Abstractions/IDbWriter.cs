using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public interface IDbWriter : IDbCommandExecutor, IAsyncDisposable, IDisposable
    {
        ValueTask<int> CommandAsync(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#if NET5_0_OR_GREATER
        ValueTask<T?> CommandAsync<T>(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#else
        ValueTask<T> CommandAsync<T>(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#endif
        ValueTask<int> StoredProcedureAsync(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#if NET5_0_OR_GREATER
        ValueTask<T?> StoredProcedureAsync<T>(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#else
        ValueTask<T> StoredProcedureAsync<T>(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
#endif
    }
}
