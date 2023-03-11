using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data.Extensions
{
    internal static class DbCommandExtensions
    {
        public static async Task<IDataReader> ExecuteReaderAsync(this IDbCommand dbCommand, CancellationToken cancellationToken = default)
        {
            if (dbCommand is SqlCommand sqlCommand)
            {
                return await sqlCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            }
            return dbCommand.ExecuteReader();
        }

        public static async Task<object> ExecuteScalarAsync(this IDbCommand dbCommand, CancellationToken cancellationToken = default)
        {
            if (dbCommand is SqlCommand sqlCommand)
            {
                return await sqlCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
            return dbCommand.ExecuteScalar();
        }

        public static async Task<int> ExecuteNonQueryAsync(this IDbCommand dbCommand, CancellationToken cancellationToken = default)
        {
            if (dbCommand is SqlCommand sqlCommand)
            {
                return await sqlCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            return dbCommand.ExecuteNonQuery();
        }
    }
}
