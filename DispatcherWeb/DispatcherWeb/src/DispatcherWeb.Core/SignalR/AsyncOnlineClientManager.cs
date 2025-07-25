using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Extensions;
using Abp.RealTime;
using Abp.Timing;
using DispatcherWeb.Infrastructure.Extensions;

namespace DispatcherWeb.SignalR
{
    public class AsyncOnlineClientManager<T> : AsyncOnlineClientManager, IOnlineClientManager<T>
    {
        public AsyncOnlineClientManager(IAsyncOnlineClientStore store) : base(store)
        {
            throw new NotImplementedException("Deprecated. Use non-generic class instead");
        }
    }

    public class AsyncOnlineClientManager : IAsyncOnlineClientManager, IOnlineClientManager
    {
        public event EventHandler<OnlineClientEventArgs> ClientConnected;
        public event EventHandler<OnlineClientEventArgs> ClientDisconnected;
        public event EventHandler<OnlineUserEventArgs> UserConnected;
        public event EventHandler<OnlineUserEventArgs> UserDisconnected;

        /// <summary>
        /// Online clients Store.
        /// </summary>
        protected IAsyncOnlineClientStore Store { get; }

        public AsyncOnlineClientManager(IAsyncOnlineClientStore store)
        {
            Store = store;
        }

        public virtual async Task AddAsync(IOnlineClient client)
        {
            client.SetLastHeartbeatDateTime(Clock.Now);

            var user = client.ToUserIdentifierOrNull();

            if (user != null && !await IsOnlineAsync(user))
            {
                UserConnected.InvokeSafely(this, new OnlineUserEventArgs(user, client));
            }

            await Store.AddOrUpdateAsync(client);

            ClientConnected.InvokeSafely(this, new OnlineClientEventArgs(client));
        }

        public virtual async Task UpdateAsync(IOnlineClient client)
        {
            client.SetLastHeartbeatDateTime(Clock.Now);
            await Store.AddOrUpdateAsync(client);
        }

        public virtual async Task<bool> RemoveAsync(string connectionId)
        {
            IOnlineClient client = default;
            var result = await Store.TryRemoveAsync(connectionId, value => client = value);
            if (!result)
            {
                return false;
            }

            if (UserDisconnected != null)
            {
                var user = client?.ToUserIdentifierOrNull();

                if (user != null && !await IsOnlineAsync(user))
                {
                    UserDisconnected.InvokeSafely(this, new OnlineUserEventArgs(user, client));
                }
            }

            ClientDisconnected?.InvokeSafely(this, new OnlineClientEventArgs(client));

            return true;
        }

        public virtual async Task RemoveAllOlderThan(DateTime cutoffDateTime)
        {
            var clients = await GetAllClientsAsync();
            var clientsToRemove = new List<IOnlineClient>();
            foreach (var client in clients)
            {
                var lastHeartbeatDateTime = client.GetLastHeartbeatDateTime();
                if (lastHeartbeatDateTime == null
                    || lastHeartbeatDateTime < cutoffDateTime)
                {
                    clientsToRemove.Add(client);
                }
            }

            if (!clientsToRemove.Any())
            {
                return;
            }

            clients = clients.Except(clientsToRemove).ToList();

            await Store.RemoveMultipleAsync(clientsToRemove.Select(x => x.ConnectionId).ToList());

            if (UserDisconnected != null)
            {
                foreach (var clientToRemoveGroup in clientsToRemove
                         .Where(x => x.UserId.HasValue)
                         .GroupBy(x => new UserIdentifier(x.TenantId, x.UserId.Value)))
                {
                    var user = clientToRemoveGroup.Key;
                    if (!IsOnline(user, clients))
                    {
                        UserDisconnected.InvokeSafely(this, new OnlineUserEventArgs(user, clientToRemoveGroup.Last()));
                    }

                }
            }

            if (ClientDisconnected != null)
            {
                foreach (var client in clientsToRemove)
                {
                    ClientDisconnected.InvokeSafely(this, new OnlineClientEventArgs(client));
                }
            }
        }

        public virtual async Task<IOnlineClient> GetByConnectionIdOrNullAsync(string connectionId)
        {
            IOnlineClient client = default;
            if (await Store.TryGetAsync(connectionId, value => client = value))
            {
                return client;
            }

            return null;
        }

        public Task<IReadOnlyList<IOnlineClient>> GetAllClientsAsync()
        {
            return Store.GetAllAsync();
        }


        public virtual async Task<IReadOnlyList<IOnlineClient>> GetAllByUserIdAsync(IUserIdentifier user)
        {
            Check.NotNull(user, nameof(user));

            var userIdentifier = new UserIdentifier(user.TenantId, user.UserId);
            var clients = await Store.GetAllByUserIdAsync(userIdentifier);

            return clients;
        }

        public static bool IsOnline(IUserIdentifier user, IEnumerable<IOnlineClient> allClients)
        {
            return allClients.FilterBy(user).Any();
        }

        public async Task<bool> IsOnlineAsync(UserIdentifier user)
        {
            return (await GetAllByUserIdAsync(user)).Any();
        }

        public async Task<bool> RemoveAsync(IOnlineClient client)
        {
            Check.NotNull(client, nameof(client));

            return await RemoveAsync(client.ConnectionId);
        }

        public void Add(IOnlineClient client)
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientManager.AddAsync instead");
        }

        public bool Remove(string connectionId)
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientManager.RemoveAsync instead");
        }

        public IOnlineClient GetByConnectionIdOrNull(string connectionId)
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientManager.GetByConnectionIdOrNullAsync instead");
        }

        public IReadOnlyList<IOnlineClient> GetAllClients()
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientManager.GetAllClientsAsync instead");
        }

        public IReadOnlyList<IOnlineClient> GetAllByUserId(IUserIdentifier user)
        {
            throw new NotImplementedException("Deprecated, use IAsyncOnlineClientManager.GetAllByUserIdAsync instead");
        }
    }
}
