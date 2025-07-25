using System;
using Abp;
using DispatcherWeb.WebPush;

namespace DispatcherWeb.SyncRequests
{
    public class FirebasePushSenderBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }

        public int? TenantId { get; set; }

        public FcmRegistrationTokenDto RegistrationToken { get; set; }

        public string JsonPayload { get; set; }

        public Guid PushMessageGuid { get; set; }
    }
}
