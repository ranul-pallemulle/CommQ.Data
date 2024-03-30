using System.Threading.Tasks;
using System.Threading;

namespace CommQ.Data
{
    public interface IDbReaderFactory
    {
        Task<IDbReader> CreateAsync(CancellationToken cancellationToken = default);
    }
}
