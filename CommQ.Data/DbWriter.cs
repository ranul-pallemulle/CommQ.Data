using CommQ.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
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

        public async ValueTask<int> CommandAsync(string command, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default)
        {
            using (var dbCommand = _unitOfWork.CreateCommand())
            {
                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = command;

                var parameters = new DbParameters(dbCommand.Parameters);
                setupParameters?.Invoke(parameters);

                return await dbCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
