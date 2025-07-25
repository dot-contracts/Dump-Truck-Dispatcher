using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.SyncRequests.Dto;

namespace DispatcherWeb.SyncRequests
{
    public interface IDriverSyncRequestStore
    {
        Task SetAsync(UpdateDriverSyncRequestTimestampInput input);
        Task SetAsync(IReadOnlyCollection<UpdateDriverSyncRequestTimestampInput> inputs);
        Task<Dictionary<string, DateTime>> GetAsync(int driverId, int? tenantId);
    }
}
