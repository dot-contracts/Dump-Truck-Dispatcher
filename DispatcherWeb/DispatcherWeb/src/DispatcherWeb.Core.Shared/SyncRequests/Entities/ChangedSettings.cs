using System;
using System.Collections.Generic;
using Abp.Timing;

namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedSettings : ChangedEntityAbstract, IChangedDriverAppEntity
    {
        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedSettings other
                && other.LastUpdateDateTime == LastUpdateDateTime
                && base.IsSame(obj);
        }

        public DateTime LastUpdateDateTime { get; set; }

        public void UpdateFromEntityReference()
        {
            LastUpdateDateTime = Clock.Now;
        }

        int? IChangedDriverAppEntity.OldDriverIdToNotify => null;

        int? IChangedDriverAppEntity.DriverId => null;

        List<int> IChangedDriverAppEntity.DriverIds => null;

        long? IChangedDriverAppEntity.UserId => null;

        bool IChangedDriverAppEntity.AffectsAllDrivers => true;
    }
}
