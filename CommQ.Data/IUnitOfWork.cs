using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        IDbCommand CreateCommand();
        IDbWriter CreateWriter();
        IDbReader CreateReader();
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
