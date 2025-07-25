using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.RealTime;
using Abp.Runtime.Caching.Redis;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DispatcherWeb.SignalR
{
    public class RedisOnlineClientStore<T> : RedisOnlineClientStore, IOnlineClientStore<T>
    {
        public RedisOnlineClientStore(IAbpRedisCacheDatabaseProvider database, AbpRedisCacheOptions options) : base(database, options)
        {
            throw new NotImplementedException("Deprecated. Use non-generic class instead.");
        }
    }

    public class RedisOnlineClientStore : IOnlineClientStore, IAsyncOnlineClientStore
    {
        public const string OnlineClientsStoreKey = "Abp.RealTime.OnlineClients";

        private readonly IAbpRedisCacheDatabaseProvider _database;

        private readonly string _clientStoreKey;

        public RedisOnlineClientStore(
            IAbpRedisCacheDatabaseProvider database,
            AbpRedisCacheOptions options)
        {
            _database = database;

            _clientStoreKey = OnlineClientsStoreKey + ".Clients";
        }

        public async Task AddOrUpdateAsync(IOnlineClient client)
        {
            var database = GetDatabase();
            await database.HashSetAsync(_clientStoreKey, new[]
            {
                new HashEntry(client.ConnectionId, client.ToString()),
            });
        }

        public async Task<bool> RemoveAsync(string connectionId)
        {
            var database = GetDatabase();

            var clientValue = await database.HashGetAsync(_clientStoreKey, connectionId);
            if (clientValue.IsNullOrEmpty)
            {
                return true;
            }

            await database.HashDeleteAsync(_clientStoreKey, connectionId);
            return true;
        }

        public async Task<bool> TryRemoveAsync(string connectionId, Action<IOnlineClient> clientAction)
        {
            try
            {
                var database = GetDatabase();

                var clientValue = await database.HashGetAsync(_clientStoreKey, connectionId);
                if (clientValue.IsNullOrEmpty)
                {
                    clientAction(null);
                    return true;
                }

                clientAction(JsonConvert.DeserializeObject<OnlineClient>(clientValue));

                await database.HashDeleteAsync(_clientStoreKey, connectionId);
                return true;
            }
            catch (Exception e)
            {
                //TODO look into these
                Console.WriteLine(e);
                clientAction(null);
                return false;
            }
        }

        public async Task<bool> RemoveMultipleAsync(ICollection<string> connectionIds)
        {
            var database = GetDatabase();
            var hashFields = connectionIds.Select(id => (RedisValue)id).ToArray();

            await database.HashDeleteAsync(_clientStoreKey, hashFields);

            return true;
        }

        public async Task<bool> TryGetAsync(string connectionId, Action<IOnlineClient> clientAction)
        {
            var database = GetDatabase();
            var clientValue = await database.HashGetAsync(_clientStoreKey, connectionId);
            if (clientValue.IsNullOrEmpty)
            {
                clientAction(null);
                return false;
            }

            clientAction(JsonConvert.DeserializeObject<OnlineClient>(clientValue));
            return true;
        }

        public async Task<IReadOnlyList<IOnlineClient>> GetAllAsync()
        {
            var database = GetDatabase();
            var clientsEntries = await database.HashGetAllAsync(_clientStoreKey);
            var clients = clientsEntries
                .Select(entry => JsonConvert.DeserializeObject<OnlineClient>(entry.Value))
                .Cast<IOnlineClient>()
                .ToList();

            return clients.ToImmutableList();
        }

        public async Task<IReadOnlyList<IOnlineClient>> GetAllByUserIdAsync(UserIdentifier userIdentifier)
        {
            var database = GetDatabase();
            var clientsEntries = await database.HashGetAllAsync(_clientStoreKey);
            var clients = new List<IOnlineClient>();
            foreach (var entry in clientsEntries)
            {
                clients.Add(JsonConvert.DeserializeObject<OnlineClient>(entry.Value));
            }

            return clients
                .Where(e => e.TenantId == userIdentifier.TenantId && e.UserId == userIdentifier.UserId)
                .ToImmutableList();
        }

        private IDatabase GetDatabase()
        {
            return _database.GetDatabase();
        }

        public void Add(IOnlineClient client)
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientStore instead");
        }

        public bool Remove(string connectionId)
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientStore instead");
        }

        public bool TryRemove(string connectionId, out IOnlineClient client)
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientStore instead");
        }

        public bool TryGet(string connectionId, out IOnlineClient client)
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientStore instead");
        }

        public bool Contains(string connectionId)
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientStore instead");
        }

        public IReadOnlyList<IOnlineClient> GetAll()
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientStore instead");
        }
    }
}
