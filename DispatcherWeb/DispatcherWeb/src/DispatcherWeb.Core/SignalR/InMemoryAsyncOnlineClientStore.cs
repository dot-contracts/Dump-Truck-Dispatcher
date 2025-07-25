using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.RealTime;

namespace DispatcherWeb.SignalR
{
    public class InMemoryAsyncOnlineClientStore<T> : InMemoryAsyncOnlineClientStore, IOnlineClientStore<T>
    {
        public InMemoryAsyncOnlineClientStore()
        {
            throw new NotImplementedException("Deprecated. Use non-generic class instead.");
        }
    }

    public class InMemoryAsyncOnlineClientStore : IOnlineClientStore, IAsyncOnlineClientStore
    {
        /// <summary>
        /// Online clients.
        /// </summary>
        protected ConcurrentDictionary<string, IOnlineClient> Clients { get; }

        public InMemoryAsyncOnlineClientStore()
        {
            Clients = new ConcurrentDictionary<string, IOnlineClient>();
        }

        public Task AddOrUpdateAsync(IOnlineClient client)
        {
            Clients.AddOrUpdate(client.ConnectionId, client, (s, o) => client);
            return Task.CompletedTask;
        }

        public Task<bool> RemoveAsync(string connectionId)
        {
            return TryRemoveAsync(connectionId, value => _ = value);
        }

        public Task<bool> TryRemoveAsync(string connectionId, Action<IOnlineClient> clientAction)
        {
            var hasRemoved = Clients.TryRemove(connectionId, out var client);
            clientAction(client);
            return Task.FromResult(hasRemoved);
        }

        public async Task<bool> RemoveMultipleAsync(ICollection<string> connectionIds)
        {
            foreach (var connectionId in connectionIds)
            {
                await RemoveAsync(connectionId);
            }

            return true;
        }

        public Task<bool> TryGetAsync(string connectionId, Action<IOnlineClient> clientAction)
        {
            var hasValue = Clients.TryGetValue(connectionId, out var client);
            clientAction(client);
            return Task.FromResult(hasValue);
        }

        public Task<bool> ContainsAsync(string connectionId)
        {
            var hasKey = Clients.ContainsKey(connectionId);
            return Task.FromResult(hasKey);
        }

        public Task<IReadOnlyList<IOnlineClient>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<IOnlineClient>>(Clients.Values.ToImmutableList());
        }

        public async Task<IReadOnlyList<IOnlineClient>> GetAllByUserIdAsync(UserIdentifier userIdentifier)
        {
            return (await GetAllAsync())
                .Where(c => c.UserId == userIdentifier.UserId && c.TenantId == userIdentifier.TenantId)
                .ToImmutableList();
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
