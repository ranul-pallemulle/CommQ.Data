using System.Data;

namespace CommQ.Data.IntegrationTests
{
    public class UnitTest1
    {
        private readonly string _connectionString = "Data Source=LEG13N;Initial Catalog=CommQDataTests;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        [Fact]
        public async Task UnitOfWorkTest()
        {
            var uowFactory = new UnitOfWorkFactory(_connectionString);

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
        public async Task ReadUncommittedTransactions()
        {
            var uowFactory = new UnitOfWorkFactory(_connectionString);

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
                parameters.Add("@Name", SqlDbType.VarChar, 200).Value = "Test2Modified";
            });

            var item1 = await dbReader1.SingleAsync<TestEntity>("SELECT * FROM dbo.TestTable WHERE Id = 2");
            var item2 = await dbReader2.SingleAsync<TestEntity>("SELECT * FROM dbo.TestTable WHERE Id = 2");

            Assert.Equal("Test2Modified", item1.Name);
            Assert.Equal("Test2Modified", item2.Name);

        }

        [Fact]
        public async Task DbReaderTest()
        {
            var uowFactory = new UnitOfWorkFactory(_connectionString);
            await using var uow = await uowFactory.CreateAsync();
            var dbWriter = uow.CreateWriter();
            await PopulateTestTable(dbWriter);
            await uow.SaveChangesAsync();


            var dbReaderFactory = new DbReaderFactory(_connectionString);

            await using (var dbReader = await dbReaderFactory.CreateAsync())
            {
                var item = await dbReader.SingleAsync<TestEntity>("SELECT * FROM dbo.TestTable WHERE Id = @Id", parameters =>
                {
                    parameters.Add("@Id", SqlDbType.Int).Value = 2;
                });

                Assert.Equal("Test2", item.Name);
            }
        }

        private async Task<int> PopulateTestTable(IDbWriter dbWriter)
        {
            await dbWriter.CommandAsync("DROP TABLE IF EXISTS dbo.TestTable");
            await dbWriter.CommandAsync("CREATE TABLE dbo.TestTable (Id INT PRIMARY KEY IDENTITY(1,1), Name VARCHAR(200))");
            var numRows = await dbWriter.CommandAsync("INSERT INTO dbo.TestTable (Name) VALUES (@Name1), (@Name2)", parameters =>
            {
                parameters.Add("@Name1", SqlDbType.VarChar, 200).Value = "Test1";
                parameters.Add("@Name2", SqlDbType.VarChar, 200).Value = "Test2";
            });

            return numRows;
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
}