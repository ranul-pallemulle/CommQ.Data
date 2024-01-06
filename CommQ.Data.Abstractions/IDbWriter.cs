using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public interface IDbWriter : IDbCommandExecutor
    {
        ValueTask<int> CommandAsync(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
        ValueTask<T> CommandAsync<T>(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
        ValueTask<int> StoredProcedureAsync(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
        ValueTask<T> StoredProcedureAsync<T>(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
    }
}
