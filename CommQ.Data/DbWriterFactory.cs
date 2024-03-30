using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class DbWriterFactory : IDbWriterFactory
    {
        private readonly IConnectionFactory _connectionFactory;
        public DbWriterFactory(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<IDbWriter> CreateAsync(CancellationToken cancellationToken = default)
        {
            var connection = await _connectionFactory.OpenAndGetAsync(cancellationToken);
            return new DbWriter(connection);
        }
    }
}
