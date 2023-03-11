using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CommQ.Data
{
    public interface IDbReadable<T>
    {
        T Read(IDataReader reader);
    }
}
