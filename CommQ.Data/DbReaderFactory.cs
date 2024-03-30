using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class DbReaderFactory : IDbReaderFactory
    {
        private readonly IConnectionFactory _connectionFactory;

        public DbReaderFactory(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IDbReader> CreateAsync(CancellationToken cancellationToken = default)
        {
            var connection = _connectionFactory.Create();
            if (connection is DbConnection conn)
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                connection.Open();
            }
            return new DbReader(connection);
        }
    }
}
