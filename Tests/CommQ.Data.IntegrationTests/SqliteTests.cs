using Microsoft.Data.Sqlite;
using System.Data;

namespace CommQ.Data.IntegrationTests
{
    public class SqliteTests
    {
        [Fact]
        public async Task UnitOfWorkTest()
        {
            var connFactory = new TestConnections().Sqlite;
            var uowFactory = new UnitOfWorkFactory(connFactory);

            await using (var uow = await uowFactory.CreateAsync())
            {
                var dbWriter = uow.CreateWriter();
                var numRows = await PopulateTestTable(dbWriter);

                Assert.Equal(2, numRows);

                var dbReader = uow.CreateReader();

                var count = await dbReader.ScalarAsync<long>("SELECT COUNT(*) FROM TestTable");
                Assert.Equal(2, count);

                await uow.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task ReadUncommittedTransactionsTest()
        {
            var connFactory = new TestConnections().SqliteSharedCache;
            var uowFactory = new UnitOfWorkFactory(connFactory);

            await using (var uow = await uowFactory.CreateAsync())
            {
                await PopulateTestTable(uow.CreateWriter());
                await uow.SaveChangesAsync();
            }

            await using var uow1 = await uowFactory.CreateAsync();
            await using var uow2 = await uowFactory.CreateAsync(IsolationLevel.ReadUncommitted);

            var dbReader1 = uow1.CreateReader();
            var dbWriter1 = uow1.CreateWriter();

            var dbReader2 = uow2.CreateReader();
            var dbWriter2 = uow2.CreateWriter();

            // should obtain a row lock
            await dbWriter1.CommandAsync("UPDATE TestTable SET Name = @Name WHERE Id = 2", parameters =>
            {
                parameters.Add("@Name", DbType.String, 200).Value = "Test2Modified";
            });

            var item1 = await dbReader1.SingleAsync<TestEntity>("SELECT * FROM TestTable WHERE Id = 2");
            var item2 = await dbReader2.SingleAsync<TestEntity>("SELECT * FROM TestTable WHERE Id = 2");

            Assert.Equal("Test2Modified", item1?.Name);
            Assert.Equal("Test2Modified", item2?.Name); // the connection string must include Cache=Shared for this to work

        }

        [Fact]
        public async Task DbReaderTest()
        {
            var connFactory = new TestConnections().Sqlite;
            var uowFactory = new UnitOfWorkFactory(connFactory);
            await using var uow = await uowFactory.CreateAsync();
            var dbWriter = uow.CreateWriter();
            await PopulateTestTable(dbWriter);
            await uow.SaveChangesAsync();

            var dbReaderFactory = new DbReaderFactory(connFactory);

            await using var dbReader = await dbReaderFactory.CreateAsync();
            var item = await dbReader.SingleAsync<TestEntity>("SELECT * FROM TestTable WHERE Id = @Id", parameters =>
            {
                parameters.Add("@Id", DbType.Int64).Value = 2;
            });

            Assert.Equal("Test2", item?.Name);

            var nonExistentItem = await dbReader.SingleAsync<TestEntity>("SELECT * FROM TestTable WHERE Name = @Name", parameters =>
            {
                parameters.Add("@Name", DbType.String, 200).Value = "NonExistent";
            });

            Assert.Null(nonExistentItem);
        }

        [Fact]
        public async Task DbWriterTest()
        {
            var connFactory = new TestConnections().Sqlite;
            var uowFactory = new UnitOfWorkFactory(connFactory);
            await using (var uow = await uowFactory.CreateAsync())
            {
                var dbWriter = uow.CreateWriter();
                await PopulateTestTable(dbWriter);
                await uow.SaveChangesAsync();
            }

            await using (var uow = await uowFactory.CreateAsync())
            {
                var dbWriter = uow.CreateWriter();

                var id = await dbWriter.CommandAsync<long>("INSERT INTO TestTable (Name) VALUES (@Name) RETURNING Id", parameters =>
                {
                    parameters.Add("@Name", DbType.String, 200).Value = "Test";
                });

                Assert.Equal(3, id);
            }
        }

        private async Task CreateTestObjects(IDbWriter dbWriter)
        {
            await dbWriter.CommandAsync("DROP TABLE IF EXISTS TestTable");

            await dbWriter.CommandAsync("CREATE TABLE TestTable (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name VARCHAR(200))");
        }

        private async Task<int> PopulateTestTable(IDbWriter dbWriter)
        {
            await CreateTestObjects(dbWriter);
            var numRows = await dbWriter.CommandAsync("INSERT INTO TestTable (Name) VALUES (@Name1), (@Name2)", parameters =>
            {
                parameters.Add("@Name1", DbType.String, 200).Value = "Test1";
                parameters.Add("@Name2", DbType.String, 200).Value = "Test2";
            });

            return numRows;
        }
    }

    internal class SqliteConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public IDbConnection Create()
        {
            return new SqliteConnection(_connectionString);
        }
    }

    internal partial class TestConnections
    {
        public IConnectionFactory Sqlite { get; } = new SqliteConnectionFactory(
            "Data Source=C:\\Users\\ranul\\AppData\\Local\\Temp\\testcommqdatasqlite.db");
        public IConnectionFactory SqliteSharedCache { get; } = new SqliteConnectionFactory(
            "Cache=Shared;Data Source=C:\\Users\\ranul\\AppData\\Local\\Temp\\testcommqdatasqlite.db");
    }
}