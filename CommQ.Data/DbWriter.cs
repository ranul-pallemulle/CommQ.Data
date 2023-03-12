using CommQ.Data.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class DbWriter : IDbWriter
    {
        private readonly IUnitOfWork _unitOfWork;

        public DbWriter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<IDataReader> RawAsync(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _unitOfWork.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = query;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);
            var reader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return reader;
        }

        public async ValueTask<IDataReader> StoredProcedureAsync(string storedProcedureName, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _unitOfWork.CreateCommand();
            dbCommand.CommandType = CommandType.StoredProcedure;
            dbCommand.CommandText = storedProcedureName;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);
            var reader = await dbCommand.ExecuteStoredProcedureAsync(cancellationToken).ConfigureAwait(false);
            return reader;
        }

        public async ValueTask<int> CommandAsync(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _unitOfWork.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = command;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);

            return await dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask<T> CommandAsync<T>(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using var dbCommand = _unitOfWork.CreateCommand();
            dbCommand.CommandType = CommandType.Text;
            dbCommand.CommandText = command;

            var parameters = new DbParameters(dbCommand.Parameters);
            setupParameters?.Invoke(parameters);

            var result = (T)await dbCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}
