using CommQ.Data.Extensions;
using System;
using System.Collections;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private IDbTransaction? _transaction;
        private bool _isTransactionDisposed = false;
        private bool _isConnectionDisposed = false;
        public UnitOfWork(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Cannot call BeginTransactionAsync while the current transaction is active");
            }
            _transaction = await _connection.BeginTransactionAsync();
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Cannot call BeginTransactionAsync while the current transaction is active");
            }
            _transaction = await _connection.BeginTransactionAsync(isolationLevel);
        }

        public IDbCommand CreateCommand()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("BeginTransactionAsync must be called before CreateCommand");
            }
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
            if (_transaction != null && !_isTransactionDisposed)
            {
                await _transaction.RollbackAsync().ConfigureAwait(false);
                await _transaction.DisposeAsync().ConfigureAwait(false);
                _isTransactionDisposed = true;
                _transaction = null;
            }

            if (!_isConnectionDisposed)
            {
                await _connection.CloseAsync().ConfigureAwait(false);
                await _connection.DisposeAsync().ConfigureAwait(false);
                _isConnectionDisposed = true;
            }

        }

        public void Dispose()
        {
            if (_transaction != null && !_isTransactionDisposed)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _isTransactionDisposed = true;
                _transaction = null;
            }

            if (!_isConnectionDisposed)
            {
                _connection.Close();
                _connection.Dispose();
                _isConnectionDisposed = true;
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("BeginTransactionAsync must be called before SaveChangesAsync");
            }
            if (_isTransactionDisposed)
            {
                throw new ObjectDisposedException(nameof(UnitOfWork));
            }

            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _isTransactionDisposed = true;
            _transaction = null;
        }
    }
}
