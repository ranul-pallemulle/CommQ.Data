using CommQ.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data.Common
{
    public class DbWriter : IDbWriter
    {
        private readonly IUnitOfWork _unitOfWork;

        public DbWriter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<int> CommandAsync(string command, Action<IDataParameterCollection> setupParameters = null, CancellationToken cancellationToken = default)
        {
            using (var dbCommand = _unitOfWork.CreateCommand())
            {
                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = command;

                setupParameters?.Invoke(dbCommand.Parameters);

                return await dbCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
