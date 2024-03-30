using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public interface IConnectionFactory
    {
        IDbConnection Create();
        async Task<IDbConnection> OpenAndGetAsync(CancellationToken cancellationToken = default)
        {
            var connection = Create();
            if (connection is DbConnection conn)
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                connection.Open();
            }
            return connection;
        }
    }
}
