using System.Data;

namespace CommQ.Data
{
    public interface IDbParameters
    {
        IDbDataParameter Add(string parameterName, DbType dbType);
        IDbDataParameter Add(string parameterName, DbType dbType, int size);
        IDbDataParameter Add(IDbDataParameter parameter);
    }
}
