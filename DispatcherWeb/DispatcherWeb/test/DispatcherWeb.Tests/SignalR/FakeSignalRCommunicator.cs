using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.RealTime;
using DispatcherWeb.Caching;
using DispatcherWeb.SignalR;
using DispatcherWeb.SignalR.Dto;
using DispatcherWeb.SyncRequests;

namespace DispatcherWeb.Tests.SignalR
{
    public class FakeSignalRCommunicator : ISignalRCommunicator
    {
        public Task SendDebugMessage(IReadOnlyList<IOnlineClient> clients, DebugMessage message)
        {
            return Task.CompletedTask;
        }

        public Task SendDebugMessage(DebugMessage message)
        {
            return Task.CompletedTask;
        }

        public Task SendListCacheSyncRequest<TListKey>(ListCacheSyncRequest<TListKey> syncRequest) where TListKey : ListCacheKey
        {
            return Task.CompletedTask;
        }

        public Task SendSyncRequest(IReadOnlyList<IOnlineClient> clients, SyncRequest syncRequest)
        {
            return Task.CompletedTask;
        }
    }
}
