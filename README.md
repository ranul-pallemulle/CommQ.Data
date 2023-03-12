# CommQ.Data

A Unit of Work implementation. Uses types from System.Data.
  
Not an ORM.

## Usage
Install the `CommQ.Data` NuGet package - https://www.nuget.org/packages/CommQ.Data/

`CommQ.Data` depends on `Microsoft.Data.SqlClient`. Therefore, installing it brings in several sub-dependencies. If you'd like to avoid this you could install the `CommQ.Data.Abstractions` NuGet package in your core projects to access the interfaces and install `CommQ.Data` elsewhere - https://www.nuget.org/packages/CommQ.Data.Abstractions/

### Creating a unit of work
Note: if you are using dependency injection, see [Dependency Injection](#dependency-injection).
```csharp
var factory = new UnitOfWorkFactory(connectionString);
await using var uow = await factory.CreateAsync();
```
A database transaction is started upon creation of the unit of work. The transaction isolation can be controlled by using the appropriate override.
```csharp
await using var uow = await factory.CreateAsync(IsolationLevel.Serializable);
```

### Using the unit of work
```csharp
var dbWriter = uow.CreateWriter(); // IDbWriter
// ... write to the database using dbWriter
await uow.SaveChangesAsync();
```
If `SaveChangesAsync` is not called before the scope of the using statement ends, the transaction will be rolled back.

### Using IDbWriter
Use IDbWriter to execute queries that modify data. Getting an instance of IDbWriter requires an IUnitOfWork.
```csharp
var numRows = await dbWriter.CommandAsync("DELETE FROM dbo.MyTable");
```
Include parameters by including the second argument.
```csharp
var numRows = await dbWriter.CommandAsync("UPDATE dbo.MyTable SET Name = @Name WHERE Id = @Id", parameters =>
{
    parameters.Add("@Id", SqlDbType.Int).Value = 1;
    parameters.Add("@Name", SqlDbType.VarChar, 200).Value = "John";
});
```
In cases where a scalar value must be returned from the database, use the generic overload of `CommandAsync`.
```csharp
var id = await dbWriter.CommandAsync<int>("INSERT INTO dbo.MyTable OUTPUT INSERTED.Id VALUES (@Name)", parameters =>
{
    parameters.Add("@Name", SqlDbType.VarChar, 200).Value = "John";
});
```

### IDbReader
Use `IDbReader` to execute queries that do not modify data. It can be used with `IUnitOfWork` to read data within the scope of the transaction, or can be used standalone without a unit of work.  
  
Creating a standalone `IDbReader`
```csharp
var factory = new DbReaderFactory(connectionString);
await using var dbReader = await factory.CreateAsync();
```
  
Creating an `IDbReader` using a unit of work
```csharp
var dbReader = uow.CreateReader();
```
Note: in this case, since the dbReader is in the scope of the transaction, it will read uncommitted changes.

### Reading data using IDbReader
To read scalar values from the database:
```csharp
var count = await dbReader.ScalarAsync<int>("SELECT COUNT(*) FROM dbo.MyTable");
```

For convenience, `IDataReader` has methods to read classes that implement `IDbReadable` and <ins>have parameterless constructors</ins>.

```csharp
// Can be some type that maps to a table or is a result of a query
public class User : IDbReadable<User>
{
    public int Id { get; set; }
    public string Name { get; set; }

    public User() { }

    public User(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public User Read(IDataReader reader)
    {
        Id = (int)reader["Id"];
        Name = (string)reader["Name"];

        return this;
    }
}
```

`IDbReader` can then be used as follows to retrieve objects.

```csharp
User user = await dbReader.SingleAsync<User>("SELECT * FROM dbo.Users WHERE Id = 2");

IEnumerable<User> users = await dbReader.EnumerableAsync<User>("SELECT * FROM dbo.Users");
```
If a class you want to map does not have a parameterless constructor, you can define a class implementing `IDataMapper`.

```csharp
internal class UserMapper : IDataMapper<User>
{
    public User Map(IDataReader reader)
    {
        var id = (int)reader["Id"];
        var name = (string)reader["Name"];

        var user = new User(id, name);
        return user;
    }
}
```
And then use the mapper with `IDbReader`.
```csharp
var mapper = new UserMapper();

var user = await dbReader.SingleAsync("SELECT * FROM dbo.Users WHERE Id = 2", mapper);

var users = await dbReader.EnumerableAsync("SELECT * FROM dbo.Users", mapper);
```
### Stored Procedures
You can execute stored procedures using both `IDbReader` and `IDbWriter`.
```csharp
IDataReader reader = await dbReader.StoredProcedureAsync("MyStoredProc", parameters =>
{
    parameters.Add("@UserId", SqlDbType.Int).Value = 1;
});
```

### Arbitrary SQL
You can execute arbitrary SQL using both `IDbReader` and `IDbWriter` by calling `RawAsync`.

```csharp
IDataReader reader = await dbReader.RawAsync("SELECT u.Name, a.City FROM dbo.Users u JOIN dbo.Address a on a.UserId = u.Id WHERE u.Id = @UserId", parameters =>
{
    parameters.Add("@UserId", SqlDbType.Int).Value = 1;
})
```
### Dependency Injection
Add an instance of `IUnitOfWorkFactory` to the DI container.

```csharp
builder.Services.AddSingleton<IUnitOfWorkFactory>(_ => 
{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    return new UnitOfWorkFactory(connectionString);
});
```

To use `IDbReader` without a unit of work, add an instance of `IDbReaderFactory` to the DI container

```csharp
builder.Services.AddSingleton<IDbReaderFactory>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    return new DbReaderFactory(connectionString);
});
```