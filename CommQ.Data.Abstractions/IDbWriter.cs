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
    }
}
