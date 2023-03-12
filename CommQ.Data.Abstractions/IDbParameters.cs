using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CommQ.Data
{
    public interface IDbParameters
    {
        IDbDataParameter Add(string parameterName, SqlDbType sqlDbType);
        IDbDataParameter Add(string parameterName, SqlDbType sqlDbType, int size);
    }
}
