using AnyoneDeveloper.ClickHouse.ConnectionPool.Connection;
using AnyoneDeveloper.ClickHouse.ConnectionPool.Repository;
using AnyoneDeveloper.ClickHouse.ConnectionPool.Repository.SelectMode;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnyoneDeveloper.ClickHouse.ConnectionPool.Demo
{
    public class Program
    {
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

        static void Main(string[] args)
        {
            //Use TCP Port.
            //If you are using Web application. Feel free to add ClickHouseConnection as Singleton. And add IClickHouseRepository as Scoped.
            //Take example here:
            //
            //[CODE]
            //services.AddScoped<IClickHouseRepository, ClickHouseRepository>();
            //services.AddSingleton(x => new ClickHouseConnection(_configuration.GetValue<int>("Connection:Count"), _configuration.GetValue<string>("ConnectionStrings:SRV"))
            //    .InitSemaphore(_configuration.GetValue<int>("Connection:InitialConcurrentCount"), _configuration.GetValue<int>("Connection:MaximumConcurrentCount")));
            //[CODE]
            
            //If you don't need DI, just new object for yourself. 
            //Parameter explain: 30 is ConnectionPool size.
            ClickHouseConnection _conn = new ClickHouseConnection(30, "Host=0.0.0.0;Port=9003;Database=Test;User=Test;Password=Test");
            //Parameter explain: They are SemaphoreSlim initCount and maxCount
            _conn.InitSemaphore(20, 20);
            IClickHouseRepository repository = new ClickHouseRepository(_conn);
            
            //Take Repository instance. Invoke GetResultAsync, passing your logic code here
            var sql = @"SELECT ID, Name, Job FROM one_table";
            var data = repository.GetResultAsync((conn) => GetSomeDataAsync(conn, sql));
            //now you have the data.
            
            Console.WriteLine("Hello World!");
        }
    }
}
