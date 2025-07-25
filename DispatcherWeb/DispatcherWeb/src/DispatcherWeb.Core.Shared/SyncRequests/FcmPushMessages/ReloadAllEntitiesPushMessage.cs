using DispatcherWeb.DriverApplication;

namespace DispatcherWeb.SyncRequests.FcmPushMessages
{
    public class ReloadAllEntitiesPushMessage : FcmPushMessageAbstract
    {
        public override FcmPushMessageType MessageType => FcmPushMessageType.ReloadAllEntities;

        public EntityEnum EntityType { get; set; }
    }
}
