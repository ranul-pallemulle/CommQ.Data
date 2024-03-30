using System.Threading.Tasks;
using System.Threading;

namespace CommQ.Data
{
    public interface IDbWriterFactory
    {
        Task<IDbWriter> CreateAsync(CancellationToken cancellationToken = default);
    }
}
