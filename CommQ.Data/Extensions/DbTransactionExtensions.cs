using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data.Extensions
{
    internal static class DbTransactionExtensions
    {
        public static async Task RollbackAsync(this IDbTransaction dbTransaction)
        {
            if (dbTransaction is SqlTransaction sqlTransaction)
            {
                await sqlTransaction.RollbackAsync().ConfigureAwait(false);
                return;
            }
            dbTransaction.Rollback();
        }

        public static async Task CommitAsync(this IDbTransaction dbTransaction, CancellationToken cancellationToken = default)
        {
            if (dbTransaction is SqlTransaction sqlTransaction)
            {
                await sqlTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return;
            }
            dbTransaction.Commit();
        }

        public static async Task DisposeAsync(this IDbTransaction dbTransaction)
        {
            if (dbTransaction is SqlTransaction sqlTransaction)
            {
                await sqlTransaction.DisposeAsync().ConfigureAwait(false);
                return;
            }
            dbTransaction.Dispose();
        }
    }
}
