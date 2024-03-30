using Npgsql;
using System.Data;

namespace CommQ.Data.IntegrationTests
{
    public class NpgsqlTests
    {
        private async Task CreateTestObjects(IDbWriter dbWriter)
        {
            await dbWriter.CommandAsync("DROP PROCEDURE IF EXISTS dbo.BasicReadProc");
            await dbWriter.CommandAsync("DROP PROCEDURE IF EXISTS dbo.BasicWriteProc");
            await dbWriter.CommandAsync("DROP TABLE IF EXISTS dbo.TestTable");

            await dbWriter.CommandAsync("CREATE TABLE dbo.TestTable (Id BIGINT PRIMARY KEY IDENTITY(1,1), Name VARCHAR(200))");
            await dbWriter.CommandAsync(@"
            CREATE PROCEDURE [dbo].[BasicReadProc]
                @name VARCHAR(20)
            AS
            BEGIN
                -- SET NOCOUNT ON added to prevent extra result sets from
                -- interfering with SELECT statements.
                SET NOCOUNT ON;

                -- Insert statements for procedure here
                SELECT Id, [Name] FROM dbo.TestTable WHERE [Name] = @name;
            END
            ");
            await dbWriter.CommandAsync(@"
            CREATE PROCEDURE [dbo].[BasicWriteProc]
                @name VARCHAR(20)
            AS
            BEGIN
                -- SET NOCOUNT ON added to prevent extra result sets from
                -- interfering with SELECT statements.
                SET NOCOUNT ON;

                -- Insert statements for procedure here
                INSERT INTO dbo.TestTable ([Name])
                VALUES (@name)
            END
            ");
        }
        private async Task<int> PopulateTestTable(IDbWriter dbWriter)
        {
            await CreateTestObjects(dbWriter);
            var numRows = await dbWriter.CommandAsync("INSERT INTO dbo.TestTable (Name) VALUES (@Name1), (@Name2)", parameters =>
            {
                parameters.Add("@Name1", DbType.String, 200).Value = "Test1";
                parameters.Add("@Name2", DbType.String, 200).Value = "Test2";
            });

            return numRows;
        }
    }

    internal class NpgsqlConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public NpgsqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public IDbConnection Create()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }

    internal partial class TestConnections
    {
        public IConnectionFactory Npgsql { get; } = new NpgsqlConnectionFactory(
            "User Id=commqtest;Password=commqtest;Host=localhost;Database=CommQDataTests");
    }
}
