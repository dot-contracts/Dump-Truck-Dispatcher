using DispatcherWeb.SyncRequests.Entities;

namespace DispatcherWeb.SyncRequests
{
    public abstract class SyncRequestChangeDetailAbstract
    {
        public EntityEnum EntityType { get; set; }
        public ChangeType ChangeType { get; set; }
    }

    public class SyncRequestChangeDetail<TEntity> : SyncRequestChangeDetailAbstract, ISyncRequestChangeDetail where TEntity : ChangedEntityAbstract
    {
        public TEntity Entity { get; set; }

        public SyncRequestChangeDetail(EntityEnum entityType, TEntity changedEntity, ChangeType changeType)
        {
            EntityType = entityType;
            Entity = changedEntity;
            ChangeType = changeType;
        }

        ChangedEntityAbstract ISyncRequestChangeDetail.Entity => Entity;
    }
}
