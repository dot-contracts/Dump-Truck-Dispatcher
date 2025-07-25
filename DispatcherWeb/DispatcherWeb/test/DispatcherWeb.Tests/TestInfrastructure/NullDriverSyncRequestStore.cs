using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.SyncRequests.Dto;

namespace DispatcherWeb.Tests.TestInfrastructure
{
    public class NullDriverSyncRequestStore : IDriverSyncRequestStore
    {
        public Task SetAsync(UpdateDriverSyncRequestTimestampInput input)
        {
            return Task.CompletedTask;
        }

        public Task SetAsync(IReadOnlyCollection<UpdateDriverSyncRequestTimestampInput> inputs)
        {
            return Task.CompletedTask;
        }

        public Task<Dictionary<string, DateTime>> GetAsync(int driverId, int? tenantId)
        {
            return Task.FromResult(new Dictionary<string, DateTime>());
        }
    }
}
