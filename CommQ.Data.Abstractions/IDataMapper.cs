using System.Data;

namespace CommQ.Data
{
    public interface IDataMapper<T>
    {
        T Map(IDataReader dataReader);
    }
}
