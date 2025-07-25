using System;

namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedEmployeeTime : ChangedDriverAppEntity
    {
        public int? TruckId { get; set; }
        public Guid? Guid { get; set; }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedEmployeeTime other
                && other.TruckId.Equals(TruckId)
                && other.Guid.Equals(Guid)
                && base.IsSame(obj);
        }
    }
}
