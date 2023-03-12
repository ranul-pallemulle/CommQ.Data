# CommQ.Data

A Unit of Work implementation. Uses types from System.Data.
  
Not an ORM.

## Usage
Install the `CommQ.Data` NuGet package - https://www.nuget.org/packages/CommQ.Data/

`CommQ.Data` depends on `Microsoft.Data.SqlClient`. Therefore, installing it brings in several sub-dependencies. If you'd like to avoid this you could install the `CommQ.Data.Abstractions` NuGet package in your core projects and install `CommQ.Data` elsewhere - https://www.nuget.org/packages/CommQ.Data.Abstractions/

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
await dbWriter.CommandAsync("DELETE FROM dbo.MyTable");
```
Include parameters by including the second argument.
```csharp
await dbWriter.CommandAsync("UPDATE dbo.MyTable SET Name = @Name WHERE Id = @Id", parameters =>
{
    parameters.Add("@Id", SqlDbType.Int).Value = 1;
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

### Reading data using IDbReader
To read scalar values from the database:
```csharp
var count = await dbReader.ScalarAsync<int>("SELECT COUNT(*) FROM dbo.MyTable");
```
  
To read arbitrary data using `IDataReader`
```csharp
IDataReader reader = await dbReader.RawAsync("SELECT u.Name, a.City FROM dbo.Users u JOIN dbo.Address a on a.UserId = u.Id WHERE u.Id = @UserId", parameters =>
{
    parameters.Add("@UserId", SqlDbType.Int).Value = 1;
})
```

For convenience, `IDataReader` has methods to read classes that implement `IDbReadable` and have parameterless constructors.

```csharp
// Can be some type that maps to a table or is a result of a query
public class User : IDbReadable<User>
{
    public int Id { get; set; }
    public string Name { get; set; }

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

### Dependency Injection
A singleton `IUnitOfWorkFactory` can be added to the DI container.

```csharp
builder.Services.AddSingleton<IUnitOfWorkFactory>(_ => 
{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    return new UnitOfWorkFactory(connectionString);
});
```

To use `IDbReader` without a unit of work, a singleton `IDbReaderFactory` can be added to the DI container

```csharp
builder.Services.AddSingleton<IDbReaderFactory>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");

    return new DbReaderFactory(connectionString);
});
```