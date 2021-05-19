using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OctonicaClient = Octonica.ClickHouseClient;

namespace AnyoneDeveloper.ClickHouse.ConnectionPool.Connection
{
    public class ClickHouseConnection : OctonicaClient.ClickHouseConnection
    {
        private static IList _conn = ArrayList.Synchronized(new List<ClickHouseConnection>());
        private static IList _states = ArrayList.Synchronized(new List<bool>());
        private static volatile int pointer = 0;
        private static volatile int connectionCount;
        private static SemaphoreSlim sema;
        private int currentIndex = 0;

        public ClickHouseConnection(string connectionString) : base(connectionString)
        {
        }

        public ClickHouseConnection(int size, string connectionString) : base(connectionString)
        {
            connectionCount = size;
            var c = 0;
            while (c < connectionCount)
            {
                var conn = new ClickHouseConnection(connectionString);
                conn.currentIndex = c;
                conn.Open();

                _conn.Add(conn);
                _states.Add(true);
                c++;
            }
        }

        public ClickHouseConnection InitSemaphore(int initCount, int maxCount)
        {
            sema = new SemaphoreSlim(initCount, maxCount);
            return this;
        }

        public int ConnectionCount()
        {
            return _conn.Count;
        }

        public int ConcurrentCount()
        {
            return sema.CurrentCount;
        }

        public static SemaphoreSlim GetSemaphore()
        {
            return sema;
        }

        public ClickHouseConnection GetConnection()
        {
            sema.Wait();
            ClickHouseConnection c = null;
            bool s;
            do
            {
                pointer %= connectionCount;
                s = (bool)_states[pointer];
                if (s)
                {
                    _states[pointer] = false;
                    c = (ClickHouseConnection)_conn[pointer];
                    Interlocked.Increment(ref pointer);
                    break;
                }
                Interlocked.Increment(ref pointer);
            }
            while (!s);
            return c;
        }

        public ClickHouseConnection SwapConnection(ClickHouseConnection conn)
        {
            conn.Release();
            return GetConnection();
        }

        public override void Close()
        {
            base.Close();
            _states[currentIndex] = true;
            sema.Release();
        }

        public void Release()
        {
            _states[currentIndex] = true;
            sema.Release();
        }

        public new async Task DisposeAsync()
        {
            sema.Dispose();
            base.Dispose();
            foreach (var i in _conn)
            {
                var o = i as ClickHouseConnection;
                await o.CloseAsync();
                await o.DisposeAsync();
            }
        }
    }
}
