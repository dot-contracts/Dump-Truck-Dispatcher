namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedDispatch : ChangedDriverAppEntity
    {
        public DispatchStatus Status { get; set; }

        public bool IsMultipleLoads { get; set; }

        public int OrderLineId { get; set; }

        public int? OrderLineTruckId { get; set; }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedDispatch other
                //&& other.UserId.Equals(UserId)
                //&& other.TruckId.Equals(TruckId)
                && base.IsSame(obj);
        }
    }
}
