using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public static class ConnectionFactoryExtensions
    {
        public static async Task<IDbConnection> OpenAndGetAsync(this IConnectionFactory factory, CancellationToken cancellationToken)
        {
            var connection = factory.Create();
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
