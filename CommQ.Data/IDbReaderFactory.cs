using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CommQ.Data
{
    public interface IDbReaderFactory
    {
        Task<IDbReader> CreateAsync(CancellationToken cancellationToken = default);
    }
}
