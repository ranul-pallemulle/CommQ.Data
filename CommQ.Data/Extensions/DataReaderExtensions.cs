using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data.Extensions
{
    internal static class DataReaderExtensions
    {
        public static async Task<bool> ReadAsync(this IDataReader dataReader, CancellationToken cancellationToken = default)
        {
            if (dataReader is SqlDataReader sqlDataReader)
            {
                return await sqlDataReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            }
            return dataReader.Read();
        }
    }
}
