using CommQ.Data.Common;
using CommQ.Data.Extensions;
using Microsoft.Data.SqlClient;
using Moq;
using System.Data;

namespace CommQ.Data.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task DbWriter()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var transaction = new Mock<IDbTransaction>();

            connection.Setup(c => c.CreateCommand()).Returns(command.Object);
            connection.Setup(c => c.BeginTransaction()).Returns(transaction.Object);

            var realCommand = new SqlCommand();
            command.Setup(c => c.Parameters).Returns(realCommand.Parameters);

            IUnitOfWork sut = new UnitOfWork(connection.Object);

            await using (var uow = sut)
            {
                connection.Verify(c => c.BeginTransaction(), Times.Once);
                var dbWriter = new DbWriter(uow);
                await dbWriter.CommandAsync("", parameters =>
                {
                    
                });
                command.Verify(c => c.ExecuteNonQuery(), Times.Once);
                transaction.Verify(t => t.Commit(), Times.Never);
                await uow.SaveChangesAsync();
                transaction.Verify(t => t.Commit(), Times.Once);
            }

            transaction.Verify(t => t.Dispose(), Times.Once);
            connection.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public async Task DbReader()
        {
            var connection = new Mock<IDbConnection>();
            var command = new Mock<IDbCommand>();
            var reader = new Mock<IDataReader>();

            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var realCommand = new SqlCommand();
            command.Setup(c => c.Parameters).Returns(realCommand.Parameters);
            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);

            reader.Setup(r => r["Id"]).Returns(2);
            reader.Setup(r => r["Name"]).Returns("TestName");


            IDbReader sut = new DbReader(connection.Object);

            await using (var dbReader = sut)
            {
                var item = await sut.SingleAsync<TestEntity>("SELECT * FROM TestEntities WHERE Id = @Id", parameters =>
                {
                    parameters.Add("@Id", SqlDbType.Int).Value = 2;
                });

                command.Verify(c => c.ExecuteReader(), Times.Once);

                Assert.Equal(2, item.Id);
                Assert.Equal("TestName", item.Name);
            }

            connection.Verify(connection => connection.Dispose(), Times.Once);

            var parameter = realCommand.Parameters["@Id"];
            Assert.Equal("@Id", parameter.ParameterName);
            Assert.Equal(2, parameter.Value);
            Assert.Equal(SqlDbType.Int, parameter.SqlDbType);

            
        }
    }

    internal class TestEntity : IDbReadable<TestEntity>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TestEntity Read(IDataReader reader)
        {
            Id = reader["Id"] as int? ?? throw new InvalidCastException("Id was null");
            Name = reader["Name"] as string ?? throw new InvalidCastException("Name was null");

            return this;
        }
    }
}