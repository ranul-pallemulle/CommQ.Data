using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class DbWriter : IDbWriter
    {
        private readonly IDbConnection? _dbConnection;
        private readonly IUnitOfWork? _uow;

        public DbWriter(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public DbWriter(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async ValueTask<IDataReader> RawAsync(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);
            if (dbCommand is DbCommand dbCommand_)
            {
                return await dbCommand_.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            }
            return dbCommand.ExecuteReader();
        }

        public async ValueTask<int> StoredProcedureAsync(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.StoredProcedure;
            dbCommand.CommandText = storedProcedureName;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);
            if (dbCommand is DbCommand dbCommand_)
            {
                return await dbCommand_.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            return dbCommand.ExecuteNonQuery();
        }

#if NET5_0_OR_GREATER
        public async ValueTask<T?> StoredProcedureAsync<T>(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
#else
        public async ValueTask<T> StoredProcedureAsync<T>(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
#endif
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.StoredProcedure;
            dbCommand.CommandText = storedProcedureName;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);

#if NET5_0_OR_GREATER
            if (dbCommand is DbCommand dbCommand_)
            {
                return (T?)await dbCommand_.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
            return (T?)dbCommand.ExecuteScalar();
#else
            if (dbCommand is DbCommand dbCommand_)
            {
                return (T)await dbCommand_.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
            return (T)dbCommand.ExecuteScalar();
#endif
        }

        public async ValueTask<int> CommandAsync(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = command;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);

            if (dbCommand is DbCommand dbCommand_)
            {
                return await dbCommand_.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            return dbCommand.ExecuteNonQuery();
        }

#if NET5_0_OR_GREATER
        public async ValueTask<T?> CommandAsync<T>(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
#else
        public async ValueTask<T> CommandAsync<T>(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
#endif
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = command;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);

#if NET5_0_OR_GREATER
            if (dbCommand is DbCommand dbCommand_)
            {
                return (T?)await dbCommand_.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
            return (T?)dbCommand.ExecuteScalar();
#else
            if (dbCommand is DbCommand dbCommand_)
            {
                return (T)await dbCommand_.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
            return (T)dbCommand.ExecuteScalar();
#endif
        }

        public async ValueTask DisposeAsync()
        {
            if (_dbConnection != null)
            {
                if (_dbConnection is DbConnection conn)
                {
                    await conn.CloseAsync().ConfigureAwait(false);
                    await conn.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    Dispose();
                }    
            }
        }

        public void Dispose()
        {
            if (_dbConnection != null)
            {
                _dbConnection.Close();
                _dbConnection.Dispose();
            }
        }
    }
}
