using CommQ.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class DbReader : IDbReader
    {
        private readonly IDbConnection? _dbConnection;
        private readonly IUnitOfWork? _uow;

        public DbReader(IDbConnection sqlConnection)
        {
            _dbConnection = sqlConnection;
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

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);
            var reader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return reader;
        }

        public async ValueTask<IDataReader> StoredProcedureAsync(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.StoredProcedure;
            dbCommand.CommandText = storedProcedureName;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);
            var reader = await dbCommand.ExecuteStoredProcedureAsync(cancellationToken).ConfigureAwait(false);
            return reader;
        }

        public async ValueTask<IEnumerable<T>> EnumerableAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class, IDbReadable<T>, new()
        {
            var data = new List<T>();
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);

            using var reader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var item = new T();
                item.Read(reader);
                data.Add(item);
            }
            return data;
        }

        public async ValueTask<IEnumerable<T>> EnumerableAsync<T>(string query, IDataMapper<T> mapper, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class
        {
            var data = new List<T>();
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);

            using var reader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var item = mapper.Map(reader);
                data.Add(item);
            }
            return data;
        }

        public async ValueTask<T> ScalarAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);

            var result = (T)await dbCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async ValueTask<T?> SingleAsync<T>(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class, IDbReadable<T>, new()
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);

            using var reader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var exists = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (exists)
            {
                var item = new T();
                item.Read(reader);
                return item;
            }
            return default;
        }

        public async ValueTask<T?> SingleAsync<T>(string query, IDataMapper<T> mapper, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default) where T : class
        {
            using var dbCommand = _uow?.CreateCommand() ?? _dbConnection!.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);

            using var reader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var exists = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (exists)
            {
                var item = mapper.Map(reader);
                return item;
            }
            return default;
        }

        public async ValueTask DisposeAsync()
        {
            if (_dbConnection != null)
            {
                await _dbConnection.CloseAsync().ConfigureAwait(false);
                await _dbConnection.DisposeAsync().ConfigureAwait(false);
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
