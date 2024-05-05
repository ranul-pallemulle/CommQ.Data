using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using Xunit.Abstractions;

namespace CommQ.Data.IntegrationTests
{
    public class NpgsqlTests
    {
        private readonly ITestOutputHelper _output;
        public NpgsqlTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task UnitOfWorkTest()
        {
            var connFactory = new TestConnections().GetNpgsql(_output);
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
            var connFactory = new TestConnections().GetNpgsql(_output);
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
            Assert.Equal("Test2", item2?.Name); // Postgres does not support read uncommitted - http://www.postgresql.org/docs/current/static/sql-set-transaction.html

        }

        [Fact]
        public async Task DbReaderTest()
        {
            var connFactory = new TestConnections().GetNpgsql(_output);
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
        public async Task StoredProcedureTest()
        {
            var connFactory = new TestConnections().GetNpgsql(_output);
            var uowFactory = new UnitOfWorkFactory(connFactory);
            await using (var uow = await uowFactory.CreateAsync())
            {
                await PopulateTestTable(uow.CreateWriter());
                await uow.SaveChangesAsync();
            }
            var dbrFactory = new DbReaderFactory(connFactory);

            await using var dbReader = await dbrFactory.CreateAsync();
            var reader = await dbReader.StoredProcedureAsync("BasicReadProc", parameters =>
            {
                parameters.Add("@name", DbType.String, 20).Value = "Test1";
            });
            var hasRows = reader.Read();
            Assert.True(hasRows);
        }

        [Fact]
        public async Task DbWriterTest()
        {
            var connFactory = new TestConnections().GetNpgsql(_output);
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

        [Fact]
        public async Task DbWriterFactoryTest()
        {
            var connFactory = new TestConnections().GetNpgsql(_output);
            var dbrFactory = new DbReaderFactory(connFactory);
            var dbwFactory = new DbWriterFactory(connFactory);

            await using (var dbWriter = await dbwFactory.CreateAsync())
            {
                await CreateTestObjects(dbWriter);
            }

            await using (var dbReader = await dbrFactory.CreateAsync())
            {
                var reader = await dbReader.StoredProcedureAsync("BasicReadProc", parameters =>
                {
                    parameters.Add("@name", DbType.String, 20).Value = "Test1";
                });
                var hasRows = reader.Read();
                Assert.False(hasRows);
            }

            await using (var dbWriter = await dbwFactory.CreateAsync())
            {
                await PopulateTestTable(dbWriter);
            }

            await using (var dbReader = await dbrFactory.CreateAsync())
            {
                var reader = await dbReader.StoredProcedureAsync("BasicReadProc", parameters =>
                {
                    parameters.Add("@name", DbType.String, 20).Value = "Test1";
                });
                var hasRows = reader.Read();
                Assert.True(hasRows);
            }
        }

        [Fact]
        public async Task DbReaderMappingTest()
        {
            var connFactory = new TestConnections().GetNpgsql(_output);
            var uowFactory = new UnitOfWorkFactory(connFactory);
            await using var uow = await uowFactory.CreateAsync();
            var dbWriter = uow.CreateWriter();
            await PopulateTestTable(dbWriter);
            await uow.SaveChangesAsync();


            var dbReaderFactory = new DbReaderFactory(connFactory);

            await using var dbReader = await dbReaderFactory.CreateAsync();
            var mapper = new TestMappedEntityMapper();
            var item = await dbReader.SingleAsync("SELECT * FROM TestTable WHERE Id = @Id", mapper, parameters =>
            {
                parameters.Add("@Id", DbType.Int64).Value = 2;
            });

            Assert.Equal("Test2", item?.Name);

            var nonExistentItem = await dbReader.SingleAsync("SELECT * FROM TestTable WHERE Name = @Name", mapper, parameters =>
            {
                parameters.Add("@Name", DbType.String, 200).Value = "NonExistent";
            });

            Assert.Null(nonExistentItem);
        }

        private async Task CreateTestObjects(IDbWriter dbWriter)
        {
            await dbWriter.CommandAsync("DROP TABLE IF EXISTS TestTable");

            await dbWriter.CommandAsync("CREATE TABLE TestTable (Id BIGSERIAL PRIMARY KEY, Name VARCHAR(200))");
            await dbWriter.CommandAsync(@"
            CREATE OR REPLACE FUNCTION BasicReadProc(IN name VARCHAR(20)) RETURNS TABLE(Id BIGINT, Name VARCHAR(200))
            LANGUAGE SQL
            AS $$
                -- Insert statements for procedure here
                SELECT Id, Name FROM TestTable WHERE Name = name;
            $$;
            ");
            await dbWriter.CommandAsync(@"
            CREATE OR REPLACE PROCEDURE BasicWriteProc(IN name VARCHAR(20))
            LANGUAGE SQL
            AS $$
                -- Insert statements for procedure here
                INSERT INTO TestTable (Name) VALUES (name);
            $$;
            ");
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

    internal class NpgsqlConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ITestOutputHelper _testOutputHelper;

        public NpgsqlConnectionFactory(string connectionString, ITestOutputHelper testOutputHelper)
        {
            _connectionString = connectionString;
            _testOutputHelper = testOutputHelper;
        }
        public IDbConnection Create()
        {
            var builder = new NpgsqlDataSourceBuilder(_connectionString);
            builder.UseLoggerFactory(LoggerFactory.Create(b =>
            {
                b.SetMinimumLevel(LogLevel.Debug);
                b.AddProvider(new XUnitLoggerProvider(_testOutputHelper));
            }));
            builder.EnableParameterLogging();
            var source = builder.Build();
            AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true); // Enable calling functions with CommandType.StoredProcedure
            return source.CreateConnection();
        }
    }

    internal partial class TestConnections
    {
        private const string _connectionString = "User Id=commqtest;Password=commqtest;Host=localhost;Database=CommQDataTests";
        public IConnectionFactory GetNpgsql(ITestOutputHelper testOutputHelper) => new NpgsqlConnectionFactory(_connectionString, testOutputHelper);
    }
}
