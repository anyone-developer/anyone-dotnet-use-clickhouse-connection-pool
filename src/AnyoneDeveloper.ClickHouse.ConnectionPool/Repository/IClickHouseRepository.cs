using AnyoneDeveloper.ClickHouse.ConnectionPool.Connection;
using System;
using System.Threading.Tasks;

namespace AnyoneDeveloper.ClickHouse.ConnectionPool.Repository
{
    public interface IClickHouseRepository
    {
        public Task<T> GetResultAsync<T>(Func<ClickHouseConnection, Task<T>> func);
    }
}
