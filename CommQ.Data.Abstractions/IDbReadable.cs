using System.Data;

namespace CommQ.Data
{
    public interface IDbReadable<T> where T : new()
    {
        T Read(IDataReader reader);
    }
}
