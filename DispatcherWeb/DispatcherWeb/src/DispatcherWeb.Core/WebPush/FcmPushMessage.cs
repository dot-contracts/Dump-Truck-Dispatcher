using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.WebPush
{
    [Table("FcmPushMessage")]
    public class FcmPushMessage : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public long ReceiverUserId { get; set; }

        public User ReceiverUser { get; set; }

        public int? ReceiverDriverId { get; set; }

        public Driver ReceiverDriver { get; set; }

        public int? FcmRegistrationTokenId { get; set; }
        public FcmRegistrationToken FcmRegistrationToken { get; set; }

        [StringLength(EntityStringFieldLengths.FcmPushMessage.JsonPayload)]
        public string JsonPayload { get; set; }

        public DateTime? SentAtDateTime { get; set; }

        public DateTime? ReceivedAtDateTime { get; set; }

        /// <summary>
        /// The value will be set for all sent but undelivered messages when another user logs in on the same device / using the same token string.
        /// Sent but undelivered: SentAtDateTime is not null && ReceivedAtDateTime is null
        /// </summary>
        public DateTime? InvalidatedAtDateTime { get; set; }

        [StringLength(EntityStringFieldLengths.FcmPushMessage.Error)]
        public string Error { get; set; }
    }
}
