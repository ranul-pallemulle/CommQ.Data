﻿using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace CommQ.Data.Extensions
{
    internal static class DbConnectionExtensions
    {
        public static async Task CloseAsync(this IDbConnection dbConnection)
        {
            if (dbConnection is SqlConnection sqlConnection)
            {
                await sqlConnection.CloseAsync().ConfigureAwait(false);
                return;
            }
            dbConnection.Close();
        }

        public static async Task DisposeAsync(this IDbConnection dbConnection)
        {
            if (dbConnection is SqlConnection sqlConnection)
            {
                await sqlConnection.DisposeAsync().ConfigureAwait(false);
                return;
            }
            dbConnection.Dispose();
        }

        public static async Task<IDbTransaction> BeginTransactionAsync(this IDbConnection dbConnection)
        {
            if (dbConnection is SqlConnection sqlConnection)
            {
                return await sqlConnection.BeginTransactionAsync().ConfigureAwait(false);
            }
            return dbConnection.BeginTransaction();
        }

        public static async Task<IDbTransaction> BeginTransactionAsync(this IDbConnection dbConnection, IsolationLevel isolationLevel)
        {
            if (dbConnection is SqlConnection sqlConnection)
            {
                return await sqlConnection.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);
            }
            return dbConnection.BeginTransaction(isolationLevel);
        }
    }
}
