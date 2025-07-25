namespace DispatcherWeb.SyncRequests.Entities
{
    public class ChangedChatMessage : ChangedDriverAppEntity<long>
    {
        public long TargetUserId { get; set; }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedChatMessage other
                && other.TargetUserId.Equals(UserId)
                && base.IsSame(obj);
        }
    }
}
