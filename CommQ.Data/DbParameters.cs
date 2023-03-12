using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

namespace CommQ.Data
{
    internal class DbParameters : IDbParameters
    {
        private readonly IDataParameterCollection _collection;

        public DbParameters(IDataParameterCollection collection)
        {
            _collection = collection;
        }
        public IDbDataParameter Add(string parameterName, SqlDbType sqlDbType)
        {
            if (_collection is SqlParameterCollection sqlCollection)
            {
                return sqlCollection.Add(parameterName, sqlDbType);
            }
            throw new NotSupportedException("Unsupported parameter collection type");
        }

        public IDbDataParameter Add(string parameterName, SqlDbType sqlDbType, int size)
        {
            if (_collection is SqlParameterCollection sqlCollection)
            {
                return sqlCollection.Add(parameterName, sqlDbType, size);
            }
            throw new NotSupportedException("Unsupported parameter collection type");
        }
    }
}
