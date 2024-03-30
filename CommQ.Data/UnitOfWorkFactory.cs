using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IConnectionFactory _connectionFactory;

        public UnitOfWorkFactory(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = default)
        {
            var uow = await ConstructUnitOfWork(cancellationToken);
            await uow.BeginTransactionAsync().ConfigureAwait(false);
            return uow;
        }

        public async Task<IUnitOfWork> CreateAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            var uow = await ConstructUnitOfWork(cancellationToken);
            await uow.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);
            return uow;
        }

        private async Task<IUnitOfWork> ConstructUnitOfWork(CancellationToken cancellationToken)
        {
            var connection = await _connectionFactory.OpenAndGetAsync(cancellationToken);
            var uow = new UnitOfWork(connection);
            return uow;
        }
    }
}
