using System.Collections.Generic;
using DispatcherWeb.DriverApplication;
using DispatcherWeb.SyncRequests.FcmPushMessages.ChangeDetails;

namespace DispatcherWeb.SyncRequests.FcmPushMessages
{
    public class ReloadSpecificEntitiesPushMessage : FcmPushMessageAbstract
    {
        public override FcmPushMessageType MessageType => FcmPushMessageType.ReloadSpecificEntities;

        public EntityEnum EntityType { get; set; }

        public List<FcmEntityChangeDetailsDto> Changes { get; set; } = new List<FcmEntityChangeDetailsDto>();
    }
}
