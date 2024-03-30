using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class DbReader : IDbReader
    {
        private readonly IDbConnection? _dbConnection;
        private readonly IUnitOfWork? _uow;

        public DbReader(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public DbReader(IUnitOfWork uow)
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

        public async ValueTask<IDataReader> StoredProcedureAsync(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.StoredProcedure;
            dbCommand.CommandText = storedProcedureName;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);
            if (dbCommand is DbCommand dbCommand_)
            {
                return await dbCommand_.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            }
            return dbCommand.ExecuteReader();
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

            if (dbCommand is DbCommand dbCommand_)
            {
#if NET5_0_OR_GREATER
                return (T?)await dbCommand_.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
#else
                return (T)await dbCommand_.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
#endif
            }
#if NET5_0_OR_GREATER
            return (T?)dbCommand.ExecuteScalar();
#else
            return (T)dbCommand.ExecuteScalar();
#endif
        }

        public async ValueTask<IEnumerable<T>> EnumerableAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class, IDbReadable<T>, new()
        {
            var data = new List<T>();
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);

            if (dbCommand is DbCommand dbCommand_)
            {
                using var reader = await dbCommand_.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var item = new T();
                    item.Read(reader);
                    data.Add(item);
                }
                return data;
            }
            else
            {
                using var reader = dbCommand.ExecuteReader();
                while (reader.Read())
                {
                    var item = new T();
                    item.Read(reader);
                    data.Add(item);
                }
                return data;
            }
        }

        public async ValueTask<IEnumerable<T>> EnumerableAsync<T>(string query, IDataMapper<T> mapper, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class
        {
            var data = new List<T>();
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);

            if (dbCommand is DbCommand dbCommand_)
            {
                using var reader = await dbCommand_.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var item = mapper.Map(reader);
                    data.Add(item);
                }
                return data;
            }
            else
            {
                using var reader = dbCommand.ExecuteReader();
                while (reader.Read())
                {
                    var item = mapper.Map(reader);
                    data.Add(item);
                }
                return data;
            }
        }

#if NET5_0_OR_GREATER
        public async ValueTask<T?> ScalarAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
#else
        public async ValueTask<T> ScalarAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
#endif
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);

#if NET5_0_OR_GREATER
            if (dbCommand is DbCommand dbCommand_)
            {
                var result = (T?)await dbCommand_.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            else
            {
                var result = (T?)dbCommand.ExecuteScalar();
                return result;
            }
#else
            if (dbCommand is DbCommand dbCommand_)
            {
                var result = (T)await dbCommand_.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            else
            {
                var result = (T)dbCommand.ExecuteScalar();
                return result;
            }
#endif
        }

        public async ValueTask<T?> SingleAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class, IDbReadable<T>, new()
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);

            if (dbCommand is DbCommand dbCommand_)
            {
                using var reader = await dbCommand_.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                var exists = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                if (exists)
                {
                    var item = new T();
                    item.Read(reader);
                    return item;
                }
                return default;
            }
            else
            {
                using var reader = dbCommand.ExecuteReader();
                var exists = reader.Read();
                if (exists)
                {
                    var item = new T();
                    item.Read(reader);
                    return item;
                }
                return default;
            }
        }

        public async ValueTask<T?> SingleAsync<T>(string query, IDataMapper<T> mapper, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand);
            setupParameters?.Invoke(parameters);

            if (dbCommand is DbCommand dbCommand_)
            {
                using var reader = await dbCommand_.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                var exists = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                if (exists)
                {
                    var item = mapper.Map(reader);
                    return item;
                }
                return default;
            }
            else
            {
                using var reader = dbCommand.ExecuteReader();
                var exists = reader.Read();
                if (exists)
                {
                    var item = mapper.Map(reader);
                    return item;
                }
                return default;
            }
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
