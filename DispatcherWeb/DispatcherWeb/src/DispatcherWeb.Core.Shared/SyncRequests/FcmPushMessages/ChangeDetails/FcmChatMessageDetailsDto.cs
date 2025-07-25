using System;

namespace DispatcherWeb.SyncRequests.FcmPushMessages.ChangeDetails
{
    public class FcmChatMessageDetailsDto : FcmEntityChangeDetailsDto<long>
    {
        public long TargetUserId { get; set; }

        public DateTime LastUpdateDateTime { get; set; }
    }
}
