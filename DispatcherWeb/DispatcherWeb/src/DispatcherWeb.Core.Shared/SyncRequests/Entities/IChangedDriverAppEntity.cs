using System;
using System.Collections.Generic;

namespace DispatcherWeb.SyncRequests.Entities
{
    public interface IChangedDriverAppEntity
    {
        int? DriverId { get; }
        List<int> DriverIds { get; }
        long? UserId { get; }
        DateTime LastUpdateDateTime { get; set; }
        int? OldDriverIdToNotify { get; }
        bool AffectsAllDrivers { get; }
        void UpdateFromEntityReference();
    }
}
