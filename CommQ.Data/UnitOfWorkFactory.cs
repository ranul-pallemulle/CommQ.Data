using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly string _connectionString;

        public UnitOfWorkFactory(string connectionString)
        {
            _connectionString = connectionString;
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
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var uow = new UnitOfWork(connection);
            return uow;
        }
    }
}
