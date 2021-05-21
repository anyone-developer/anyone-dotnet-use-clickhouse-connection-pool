# anyone-dotnet-use-clickhouse-connection-pool

This nuget package help you to use Octonica.ClickHouseClient with connection pool. You can fully control the concurrent query with "select" I/O mode. The original client repo is here: 
https://github.com/Octonica/ClickHouseClient

This repo would bundle nuget package. Anyone can reference it from *nuget.org*.

nuget package:
https://www.nuget.org/packages/AnyoneDeveloper.ClickHouse.ConnectionPool/

*If you like my module, please buy me a coffee.*

*More and more tiny and useful GitHub action modules are on the way. Please donate to me. I accept a part-time job contract. if you need, please contact me: zhang_nan_163@163.com*

## How to use

### Inject referenced class

- Add *AnyoneDeveloper.ClickHouse.ConnectionPool* package reference.
- Inject Repository and Connection class
  - Inject Repository class
  ```csharp
    services.AddScoped<IClickHouseRepository, ClickHouseRepository>();
  ```
  - Inject Connection class
  ```csharp
    services.AddSingleton(x => new ClickHouseConnection(_configuration.GetValue<int>("Connection:Count"), _configuration.GetValue<string>("ConnectionStrings:SRV"))
         .InitSemaphore(_configuration.GetValue<int>("Connection:InitialConcurrentCount"), _configuration.GetValue<int>("Connection:MaximumConcurrentCount")));
  ```

### Set up configuration

- Set up connection pool size
  It should greater than concurrent size.
- Set up SemaphoreSlim InitCount and MaxCount

### Define execution method

```csharp
  private static async Task<IEnumerable<Foo>> GetSomeDataAsync(ClickHouseConnection conn, string sql)
  {
      using var cmd = conn.CreateCommand(sql);
      var items = new List<Foo>();
      using (var reader = await cmd.ExecuteReaderAsync())
      {
          while (await reader.ReadAsync())
          {
              var item = new Foo();
              item.ID = await reader.GetFieldValueAsync<int>(0);
              item.Name = await reader.GetFieldValueAsync<string>(1);
              item.Job = await reader.GetFieldValueAsync<string>(2);

              items.Add(item);
          }
      }
      return items;
  }
```

### Use it as below code:

```csharp
  var sql = @"SELECT ID, Name, Job FROM one_table";
  var data = repository.GetResultAsync((conn) => GetSomeDataAsync(conn, sql));
```

### You can reference complete code from Demo

## Donation

PalPal: https://paypal.me/nzhang4

<img src="https://raw.githubusercontent.com/anyone-developer/anyone-dotnet-use-grpc-ui/main/misc/alipay.JPG" width="500">

<img src="https://raw.githubusercontent.com/anyone-developer/anyone-dotnet-use-grpc-ui/main/misc/webchat_pay.JPG" width="500">


