using DispatcherWeb.SyncRequests.Entities;

namespace DispatcherWeb.SyncRequests
{
    public interface ISyncRequestChangeDetail
    {
        ChangedEntityAbstract Entity { get; }
        EntityEnum EntityType { get; }
        ChangeType ChangeType { get; }
    }
}
