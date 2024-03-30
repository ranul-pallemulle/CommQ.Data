using System.Data;

namespace CommQ.Data
{
    public interface IConnectionFactory
    {
        IDbConnection Create();
    }
}
