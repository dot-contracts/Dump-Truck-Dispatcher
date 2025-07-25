using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.RealTime;

namespace DispatcherWeb.SignalR
{
    public interface IAsyncOnlineClientManager
    {
        event EventHandler<OnlineClientEventArgs> ClientConnected;

        event EventHandler<OnlineClientEventArgs> ClientDisconnected;

        event EventHandler<OnlineUserEventArgs> UserConnected;

        event EventHandler<OnlineUserEventArgs> UserDisconnected;

        /// <summary>
        /// Adds a client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>The task to handle async operation</returns>
        Task AddAsync(IOnlineClient client);

        Task UpdateAsync(IOnlineClient client);

        /// <summary>
        /// Removes a client by connection id.
        /// </summary>
        /// <param name="connectionId">The connection id.</param>
        /// <returns>True, if a client is removed</returns>
        Task<bool> RemoveAsync(string connectionId);

        Task RemoveAllOlderThan(DateTime cutoffDateTime);

        /// <summary>
        /// Tries to find a client by connection id.
        /// Returns null if not found.
        /// </summary>
        /// <param name="connectionId">connection id</param>
        Task<IOnlineClient> GetByConnectionIdOrNullAsync(string connectionId);

        /// <summary>
        /// Gets all online clients asynchronously.
        /// </summary>
        Task<IReadOnlyList<IOnlineClient>> GetAllClientsAsync();

        /// <summary>
        /// Gets all online clients by user id asynchronously.
        /// </summary>
        /// <param name="user">user identifier</param>
        Task<IReadOnlyList<IOnlineClient>> GetAllByUserIdAsync(IUserIdentifier user);

        Task<bool> IsOnlineAsync(UserIdentifier user);

        Task<bool> RemoveAsync(IOnlineClient client);
    }
}
