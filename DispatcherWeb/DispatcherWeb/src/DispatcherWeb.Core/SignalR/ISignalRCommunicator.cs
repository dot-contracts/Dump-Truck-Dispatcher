using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.RealTime;
using DispatcherWeb.Caching;
using DispatcherWeb.SignalR.Dto;
using DispatcherWeb.SyncRequests;

namespace DispatcherWeb.SignalR
{
    public interface ISignalRCommunicator
    {
        Task SendDebugMessage(IReadOnlyList<IOnlineClient> clients, DebugMessage message);
        Task SendDebugMessage(DebugMessage message);
        Task SendListCacheSyncRequest<TListKey>(ListCacheSyncRequest<TListKey> syncRequest) where TListKey : ListCacheKey;
        Task SendSyncRequest(IReadOnlyList<IOnlineClient> clients, SyncRequest syncRequest);
    }
}
