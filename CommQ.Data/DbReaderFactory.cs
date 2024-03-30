using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public interface IDbReaderFactory
    {
        Task<IDbReader> CreateAsync(CancellationToken cancellationToken = default);
    }

    public class DbReaderFactory : IDbReaderFactory
    {
        private readonly IConnectionFactory _connectionFactory;

        public DbReaderFactory(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IDbReader> CreateAsync(CancellationToken cancellationToken = default)
        {
            var connection = await _connectionFactory.OpenAndGetAsync(cancellationToken);
            return new DbReader(connection);
        }
    }
}
