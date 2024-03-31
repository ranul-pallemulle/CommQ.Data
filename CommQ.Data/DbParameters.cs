using System.Data;

namespace CommQ.Data
{
    public interface IDbParameters
    {
        IDbCommand Command { get; }
        IDbDataParameter Add(string parameterName, DbType dbType);
        IDbDataParameter Add(string parameterName, DbType dbType, int size);
        IDbDataParameter Add(IDbDataParameter parameter);
    }

    public class DbParameters : IDbParameters
    {
        private readonly IDbCommand _command;

        public DbParameters(IDbCommand command)
        {
            _command = command;
        }
        public IDbCommand Command => _command;
        public IDbDataParameter Add(string parameterName, DbType dbType)
        {
            var parameter = _command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = dbType;
            return Add(parameter);
        }

        public IDbDataParameter Add(string parameterName, DbType dbType, int size)
        {
            var parameter = _command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = dbType;
            parameter.Size = size;
            return Add(parameter);
        }

        public IDbDataParameter Add(IDbDataParameter parameter)
        {
            _command.Parameters.Add(parameter);
            return parameter;
        }
    }
}
