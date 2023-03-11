using CommQ.Data.Extensions;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private bool _isTransactionDisposed = false;
        private bool _isConnectionDisposed = false;
        public UnitOfWork(IDbConnection connection)
        {
            _connection = connection;
            _transaction = _connection.BeginTransaction();
        }

        public UnitOfWork(IDbConnection connection, IsolationLevel isolationLevel)
        {
            _connection = connection;
            _transaction = _connection.BeginTransaction(isolationLevel);
        }

        public IDbCommand CreateCommand()
        {
            if (_isTransactionDisposed || _isConnectionDisposed)
            {
                throw new ObjectDisposedException(nameof(UnitOfWork));
            }
            var command = _connection.CreateCommand();
            command.Transaction = _transaction;
            return command;
        }

        public IDbReader CreateReader()
        {
            return new DbReader(this);
        }

        public IDbWriter CreateWriter()
        {
            return new DbWriter(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_isTransactionDisposed)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _isTransactionDisposed = true;
            }

            if (!_isConnectionDisposed)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
                _isConnectionDisposed = true;
            }

        }

        public async void Dispose()
        {
            if (!_isTransactionDisposed)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _isTransactionDisposed = true;
            }

            if (!_isConnectionDisposed)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
                _isConnectionDisposed = true;
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_isTransactionDisposed)
            {
                throw new ObjectDisposedException(nameof(UnitOfWork));
            }

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _isTransactionDisposed = true;
        }
    }
}
