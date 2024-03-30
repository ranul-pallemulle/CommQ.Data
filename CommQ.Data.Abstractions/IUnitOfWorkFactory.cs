using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public interface IUnitOfWorkFactory
    {
        Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = default);
        Task<IUnitOfWork> CreateAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    }
}
