namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedEmployeeTimeClassification : ChangedDriverAppEntity
    {
        public int TimeClassificationId { get; set; }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedEmployeeTimeClassification other
                && other.TimeClassificationId.Equals(TimeClassificationId)
                && base.IsSame(obj);
        }
    }
}