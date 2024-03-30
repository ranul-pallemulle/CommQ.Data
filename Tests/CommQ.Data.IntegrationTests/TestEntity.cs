using System.Data;

namespace CommQ.Data.IntegrationTests
{
    internal class TestEntity : IDbReadable<TestEntity>
    {
        public TestEntity()
        {

        }

        public TestEntity(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public TestEntity Read(IDataReader reader)
        {
            Id = (long)reader["Id"];
            Name = (string)reader["Name"];

            return this;
        }
    }

    internal class TestMappedEntity
    {
        public TestMappedEntity(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public long Id { get; set; }
        public string Name { get; set; }
    }

    internal class TestMappedEntityMapper : IDataMapper<TestMappedEntity>
    {
        public TestMappedEntity Map(IDataReader dataReader)
        {
            var id = (long)dataReader["Id"];
            var name = (string)dataReader["Name"];

            var entity = new TestMappedEntity(id, name);
            return entity;
        }
    }
}