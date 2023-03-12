using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public class DbReaderFactory : IDbReaderFactory
    {
        private readonly string _connectionString;

        public DbReaderFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDbReader> CreateAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return new DbReader(connection);
        }
    }
}
