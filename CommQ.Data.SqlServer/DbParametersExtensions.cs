using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace CommQ.Data.SqlServer
{
    public static class DbParametersExtensions
    {
        public static SqlParameter Add(this IDbParameters parameters, string parameterName, SqlDbType sqlDbType)
        {
            if (parameters.Command is SqlCommand command)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.SqlDbType = sqlDbType;
                command.Parameters.Add(parameter);
                return parameter;
            }
            throw new InvalidOperationException($"Expected '{typeof(SqlCommand)}'");
        }

        public static SqlParameter Add(this IDbParameters parameters, string parameterName, SqlDbType sqlDbType, int size)
        {
            if (parameters.Command is SqlCommand command)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.SqlDbType = sqlDbType;
                parameter.Size = size;
                command.Parameters.Add(parameter);
                return parameter;
            }
            throw new InvalidOperationException($"Expected '{typeof(SqlCommand)}'");
        }
    }
}
