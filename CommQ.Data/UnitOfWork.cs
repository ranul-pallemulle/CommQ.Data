using System;
using System.Data;
using System.Data.Common;
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

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Cannot call BeginTransactionAsync while the current transaction is active");
            }
            if (_connection is DbConnection conn)
            {
                _transaction = await conn.BeginTransactionAsync(cancellationToken);
            }
            else
            {
                _transaction = _connection.BeginTransaction();
            }
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Cannot call BeginTransactionAsync while the current transaction is active");
            }
            if (_connection is DbConnection conn)
            {
                _transaction = await conn.BeginTransactionAsync(isolationLevel, cancellationToken);
            }
            else
            {
                _transaction = _connection.BeginTransaction(isolationLevel);
            }
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
                if (_transaction is DbTransaction tran)
                {
                    await tran.RollbackAsync().ConfigureAwait(false);
                    await tran.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    _transaction.Rollback();
                    _transaction.Dispose();
                }
                _isTransactionDisposed = true;
                _transaction = null;
            }

            if (!_isConnectionDisposed)
            {
                if (_connection is DbConnection conn)
                {
                    await conn.CloseAsync().ConfigureAwait(false);
                    await conn.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    _connection.Close();
                    _connection.Dispose();
                }
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

            if (_transaction is DbTransaction tran)
            {
                await tran.CommitAsync(cancellationToken).ConfigureAwait(false);
                await tran.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                _transaction.Commit();
                _transaction.Dispose();
            }
            _isTransactionDisposed = true;
            _transaction = null;
        }
    }
}
