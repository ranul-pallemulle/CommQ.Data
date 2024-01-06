using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CommQ.Data
{
    public interface IDbCommandExecutor
    {
        ValueTask<IDataReader> RawAsync(string query, Action<IDbParameters>? setupParameters = null, CancellationToken cancellationToken = default);
    }
}
