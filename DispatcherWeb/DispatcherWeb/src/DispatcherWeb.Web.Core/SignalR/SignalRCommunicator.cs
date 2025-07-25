using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.RealTime;
using Abp.Runtime.Session;
using Castle.Core.Logging;
using DispatcherWeb.Caching;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.SignalR;
using DispatcherWeb.SignalR.Dto;
using DispatcherWeb.SyncRequests;
using Microsoft.AspNetCore.SignalR;

namespace DispatcherWeb.Web.SignalR
{
    public class SignalRCommunicator : ISignalRCommunicator, ITransientDependency
    {
        public ILogger Logger { get; set; }
        private readonly IHubContext<SignalRHub> _signalrHub;
        private readonly IExtendedAbpSession _session;
        private readonly IAsyncOnlineClientManager _onlineClientManager;

        public SignalRCommunicator(
            IHubContext<SignalRHub> signalrHub,
            IExtendedAbpSession session,
            IAsyncOnlineClientManager onlineClientManager
        )
        {
            _signalrHub = signalrHub;
            _session = session;
            _onlineClientManager = onlineClientManager;
            Logger = NullLogger.Instance;
        }

        public async Task SendSyncRequest(IReadOnlyList<IOnlineClient> clients, SyncRequest syncRequest)
        {
            var signalRClients = _signalrHub.Clients.Clients(clients.Select(c => c.ConnectionId));
            if (signalRClients != null)
            {
                await signalRClients.SendAsync("syncRequest", syncRequest.ToDto());
            }
        }

        public async Task SendListCacheSyncRequest<TListKey>(ListCacheSyncRequest<TListKey> syncRequest)
            where TListKey : ListCacheKey
        {
            var group = syncRequest.TenantId.HasValue
                ? DispatcherWebConsts.SignalRGroups.ListCacheSyncRequest.Tenant(syncRequest.TenantId)
                : DispatcherWebConsts.SignalRGroups.ListCacheSyncRequest.AllTenants;

            await _signalrHub.Clients.Group(group).SendAsync("listCacheSyncRequest", syncRequest);
        }

        public async Task SendDebugMessage(DebugMessage message)
        {
            if (!_session.UserId.HasValue)
            {
                return;
            }
            var clients = await _onlineClientManager.GetAllByUserIdAsync(await _session.ToUserIdentifierAsync());
            await SendDebugMessage(clients, message);
        }

        public async Task SendDebugMessage(IReadOnlyList<IOnlineClient> clients, DebugMessage message)
        {
            var signalRClients = _signalrHub.Clients.Clients(clients.Select(c => c.ConnectionId));
            if (signalRClients != null)
            {
                await signalRClients.SendAsync("debugMessage", message);
            }
        }
    }
}
