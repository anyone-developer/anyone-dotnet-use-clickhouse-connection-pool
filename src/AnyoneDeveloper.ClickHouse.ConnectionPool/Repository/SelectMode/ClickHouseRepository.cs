using AnyoneDeveloper.ClickHouse.ConnectionPool.Connection;
using AnyoneDeveloper.ClickHouse.ConnectionPool.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Threading.Tasks;

namespace AnyoneDeveloper.ClickHouse.ConnectionPool.Repository.SelectMode
{
    public class ClickHouseRepository : IClickHouseRepository
    {
        private ClickHouseConnection _conn;

        public ClickHouseRepository(ClickHouseConnection conn)
        {
            _conn = conn;
        }

        public async Task<T> GetResultAsync<T>(Func<ClickHouseConnection, Task<T>> func)
        {
            var c = _conn.GetConnection();
            try
            {
                if (c.State.Equals(ConnectionState.Closed) || c.State.Equals(ConnectionState.Broken))
                    c.Open();
                else if (c.State.Equals(ConnectionState.Connecting) || c.State.Equals(ConnectionState.Executing) || c.State.Equals(ConnectionState.Fetching))
                {
                    while (!c.State.Equals(ConnectionState.Open))
                    {
                        c = _conn.SwapConnection(c);
                        if (c.State.Equals(ConnectionState.Closed) || c.State.Equals(ConnectionState.Broken))
                            c.Open();
                    }
                }
                return await func(c);
            }
            finally
            {
                c.Release();
            }
        }
    }
}
