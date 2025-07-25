namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedTimeClassification : ChangedDriverAppEntity
    {
        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedTimeClassification other
                && base.IsSame(obj);
        }
    }
}
