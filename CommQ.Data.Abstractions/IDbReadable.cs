using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CommQ.Data
{
    public interface IDbReadable<T> where T : new()
    {
        T Read(IDataReader reader);
    }
}
