using System.Collections;
using System.Data;

namespace CommQ.Data.IntegrationTests
{
    internal partial class TestConnections : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var connections = typeof(TestConnections)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(IConnectionFactory))
                .Select(p => new object[] { p.GetValue(this)! });
            foreach (var connection in connections)
            {
                yield return connection;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}