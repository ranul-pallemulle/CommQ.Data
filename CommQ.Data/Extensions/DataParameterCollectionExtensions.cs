using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;

namespace CommQ.Data.Extensions
{
    public static class DataParameterCollectionExtensions
    {
        public static IDbDataParameter Add(this IDataParameterCollection collection, string parameterName, SqlDbType sqlDbType)
        {
            if (collection is SqlParameterCollection sqlCollection)
            {
                return sqlCollection.Add(parameterName, sqlDbType);
            }
            throw new NotSupportedException("Unsupported parameter collection type");
        }

        public static IDbDataParameter Add(this IDataParameterCollection collection, string parameterName, SqlDbType sqlDbType, int size)
        {
            if (collection is SqlParameterCollection sqlCollection)
            {
                return sqlCollection.Add(parameterName, sqlDbType, size);
            }
            throw new NotSupportedException("Unsupported parameter collection type");
        }
    }
}
