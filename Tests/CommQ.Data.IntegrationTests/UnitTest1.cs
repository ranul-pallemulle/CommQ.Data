using Microsoft.Data.SqlClient;
using System.Data;

namespace CommQ.Data.IntegrationTests
{
    public class UnitTest1
    {
        private readonly string _connectionString = "Data Source=LEG13N;Initial Catalog=CommQDataTests;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        [Fact]
        public async Task UnitOfWorkTest()
        {
            var connFactory = new SqlServerConnectionFactory(_connectionString);
            var uowFactory = new UnitOfWorkFactory(connFactory);

            await using (var uow = await uowFactory.CreateAsync())
            {
                var dbWriter = uow.CreateWriter();
                var numRows = await PopulateTestTable(dbWriter);

                Assert.Equal(2, numRows);

                var dbReader = uow.CreateReader();

                var count = await dbReader.ScalarAsync<int>("SELECT COUNT(*) FROM dbo.TestTable");
                Assert.Equal(2, count);

                await uow.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task ReadUncommittedTransactionsTest()
        {
            var connFactory = new SqlServerConnectionFactory(_connectionString);
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
            await dbWriter1.CommandAsync("UPDATE dbo.TestTable SET Name = @Name WHERE Id = 2", parameters =>
            {
                parameters.Add("@Name", DbType.String, 200).Value = "Test2Modified";
            });

            var item1 = await dbReader1.SingleAsync<TestEntity>("SELECT * FROM dbo.TestTable WHERE Id = 2");
            var item2 = await dbReader2.SingleAsync<TestEntity>("SELECT * FROM dbo.TestTable WHERE Id = 2");

            Assert.Equal("Test2Modified", item1?.Name);
            Assert.Equal("Test2Modified", item2?.Name);

        }

        [Fact]
        public async Task DbReaderTest()
        {
            var connFactory = new SqlServerConnectionFactory(_connectionString);
            var uowFactory = new UnitOfWorkFactory(connFactory);
            await using var uow = await uowFactory.CreateAsync();
            var dbWriter = uow.CreateWriter();
            await PopulateTestTable(dbWriter);
            await uow.SaveChangesAsync();

            var dbReaderFactory = new DbReaderFactory(connFactory);

            await using var dbReader = await dbReaderFactory.CreateAsync();
            var item = await dbReader.SingleAsync<TestEntity>("SELECT * FROM dbo.TestTable WHERE Id = @Id", parameters =>
            {
                parameters.Add("@Id", DbType.Int32).Value = 2;
            });

            Assert.Equal("Test2", item?.Name);

            var nonExistentItem = await dbReader.SingleAsync<TestEntity>("SELECT * FROM dbo.TestTable WHERE Name = @Name", parameters =>
            {
                parameters.Add("@Name", DbType.String, 200).Value = "NonExistent";
            });

            Assert.Null(nonExistentItem);
        }

        [Fact]
        public async Task StoredProcedureTest()
        {
            var connFactory = new SqlServerConnectionFactory(_connectionString);
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
            var connFactory = new SqlServerConnectionFactory(_connectionString);
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

                var id = await dbWriter.CommandAsync<int>("INSERT INTO dbo.TestTable (Name) OUTPUT INSERTED.Id VALUES (@Name)", parameters =>
                {
                    parameters.Add("@Name", DbType.String, 200).Value = "Test";
                });

                Assert.Equal(3, id);
            }
        }

        [Fact]
        public async Task DbReaderMappingTest()
        {
            var connFactory = new SqlServerConnectionFactory(_connectionString);
            var uowFactory = new UnitOfWorkFactory(connFactory);
            await using var uow = await uowFactory.CreateAsync();
            var dbWriter = uow.CreateWriter();
            await PopulateTestTable(dbWriter);
            await uow.SaveChangesAsync();


            var dbReaderFactory = new DbReaderFactory(connFactory);

            await using var dbReader = await dbReaderFactory.CreateAsync();
            var mapper = new TestMappedEntityMapper();
            var item = await dbReader.SingleAsync("SELECT * FROM dbo.TestTable WHERE Id = @Id", mapper, parameters =>
            {
                parameters.Add("@Id", DbType.Int32).Value = 2;
            });

            Assert.Equal("Test2", item?.Name);

            var nonExistentItem = await dbReader.SingleAsync("SELECT * FROM dbo.TestTable WHERE Name = @Name", mapper, parameters =>
            {
                parameters.Add("@Name", DbType.String, 200).Value = "NonExistent";
            });

            Assert.Null(nonExistentItem);
        }

        private async Task<int> PopulateTestTable(IDbWriter dbWriter)
        {
            await dbWriter.CommandAsync("DROP PROCEDURE IF EXISTS dbo.BasicReadProc");
            await dbWriter.CommandAsync("DROP PROCEDURE IF EXISTS dbo.BasicWriteProc");
            await dbWriter.CommandAsync("DROP TABLE IF EXISTS dbo.TestTable");
            await dbWriter.CommandAsync("CREATE TABLE dbo.TestTable (Id INT PRIMARY KEY IDENTITY(1,1), Name VARCHAR(200))");
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
            var numRows = await dbWriter.CommandAsync("INSERT INTO dbo.TestTable (Name) VALUES (@Name1), (@Name2)", parameters =>
            {
                parameters.Add("@Name1", DbType.String, 200).Value = "Test1";
                parameters.Add("@Name2", DbType.String, 200).Value = "Test2";
            });

            return numRows;
        }
    }

    internal class SqlServerConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public SqlServerConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }

    internal class TestEntity : IDbReadable<TestEntity>
    {
        public TestEntity()
        {

        }

        public TestEntity(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public TestEntity Read(IDataReader reader)
        {
            Id = (int)reader["Id"];
            Name = (string)reader["Name"];

            return this;
        }
    }

    internal class TestMappedEntity
    {
        public TestMappedEntity(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }

    internal class TestMappedEntityMapper : IDataMapper<TestMappedEntity>
    {
        public TestMappedEntity Map(IDataReader dataReader)
        {
            var id = (int)dataReader["Id"];
            var name = (string)dataReader["Name"];

            var entity = new TestMappedEntity(id, name);
            return entity;
        }
    }
}