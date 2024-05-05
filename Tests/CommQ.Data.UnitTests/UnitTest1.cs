using Microsoft.Data.SqlClient;
using Moq;
using System.Data;

namespace CommQ.Data.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task DisposeConnectionTest()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var transaction = new Mock<IDbTransaction>();

            connection.Setup(c => c.CreateCommand()).Returns(command.Object);
            connection.Setup(c => c.BeginTransaction()).Returns(transaction.Object);

            IUnitOfWork sut = new UnitOfWork(connection.Object, disposeConnection: true);
            await sut.BeginTransactionAsync();

            await using (var uow = sut)
            {

            }

            transaction.Verify(t => t.Dispose(), Times.Once);
            connection.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public async Task PreventDisposeConnectionTest()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var transaction = new Mock<IDbTransaction>();

            connection.Setup(c => c.CreateCommand()).Returns(command.Object);
            connection.Setup(c => c.BeginTransaction()).Returns(transaction.Object);

            IUnitOfWork sut = new UnitOfWork(connection.Object, disposeConnection: false);
            await sut.BeginTransactionAsync();

            await using (var uow = sut)
            {

            }

            transaction.Verify(t => t.Dispose(), Times.Once);
            connection.Verify(c => c.Dispose(), Times.Never);
        }

        [Fact]
        public async Task DbWriterTest()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var transaction = new Mock<IDbTransaction>();

            connection.Setup(c => c.CreateCommand()).Returns(command.Object);
            connection.Setup(c => c.BeginTransaction()).Returns(transaction.Object);

            var realCommand = new SqlCommand();
            command.Setup(c => c.Parameters).Returns(realCommand.Parameters);

            IUnitOfWork sut = new UnitOfWork(connection.Object, disposeConnection: true);
            connection.Verify(c => c.BeginTransaction(), Times.Never());

            await sut.BeginTransactionAsync();
            connection.Verify(c => c.BeginTransaction(), Times.Once);

            await using (var uow = sut)
            {
                var dbWriter = new DbWriter(uow);
                await dbWriter.CommandAsync("", parameters =>
                {
                    
                });
                command.Verify(c => c.ExecuteNonQuery(), Times.Once);
                transaction.Verify(t => t.Commit(), Times.Never);
                await uow.SaveChangesAsync();
                transaction.Verify(t => t.Commit(), Times.Once);

                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await uow.SaveChangesAsync();
                });
            }

            transaction.Verify(t => t.Dispose(), Times.Once);
            connection.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public async Task DbReaderTest()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();

            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var realCommand = new SqlCommand();
            command.Setup(c => c.Parameters).Returns(realCommand.Parameters);
            command.Setup(c => c.CreateParameter()).Returns(() => realCommand.CreateParameter());
            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);

            reader.Setup(r => r.Read()).Returns(true);
            reader.Setup(r => r["Id"]).Returns(2);
            reader.Setup(r => r["Name"]).Returns("TestName");


            IDbReader sut = new DbReader(connection.Object);

            await using (var dbReader = sut)
            {
                var item = await dbReader.SingleAsync<TestEntity>("SELECT * FROM TestEntities WHERE Id = @Id", parameters =>
                {
                    parameters.Add("@Id", DbType.Int32).Value = 2;
                });

                command.Verify(c => c.ExecuteReader(), Times.Once);

                Assert.Equal(2, item?.Id);
                Assert.Equal("TestName", item?.Name);
            }

            connection.Verify(connection => connection.Dispose(), Times.Once);

            var parameter = realCommand.Parameters["@Id"];
            Assert.Equal("@Id", parameter.ParameterName);
            Assert.Equal(2, parameter.Value);
            Assert.Equal(SqlDbType.Int, parameter.SqlDbType);

            
        }

        [Fact]
        public async Task StoredProcedureTest()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();

            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            IDbReader sut = new DbReader(connection.Object);

            await using (var dbReader = sut)
            {
                await dbReader.StoredProcedureAsync("TestProc");
            }

            command.VerifySet(c => c.CommandText = "TestProc");
            command.VerifySet(c => c.CommandType = CommandType.StoredProcedure);
            command.Verify(c => c.ExecuteReader(), Times.Once);
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