namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedDriverAssignment : ChangedDriverAppEntity
    {
        public int TruckId { get; set; }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedDriverAssignment other
                && other.TruckId.Equals(TruckId)
                && base.IsSame(obj);
        }
    }
}
