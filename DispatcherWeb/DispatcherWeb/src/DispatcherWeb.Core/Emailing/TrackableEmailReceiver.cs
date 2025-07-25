using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;

namespace DispatcherWeb.Emailing
{
    public class TrackableEmailReceiver : Entity, IMayHaveTenant
    {
        public static class FieldLengthes
        {
            public const int Email = 256;
        }

        public TrackableEmailReceiver()
        {
            Events = new HashSet<TrackableEmailEvent>();
        }

        public int? TenantId { get; set; }

        public Guid TrackableEmailId { get; set; }

        [StringLength(FieldLengthes.Email)]
        public string Email { get; set; }

        public int Order { get; set; }

        public bool IsSender { get; set; }

        public EmailReceiverKind ReceiverKind { get; set; }

        public EmailDeliveryStatus DeliveryStatus { get; set; }

        public virtual TrackableEmail TrackableEmail { get; set; }

        public virtual ICollection<TrackableEmailEvent> Events { get; set; }

        public void TruncateFieldsIfNeeded()
        {
            if (Email?.Length > FieldLengthes.Email)
            {
                Email = Email.Substring(0, FieldLengthes.Email);
            }
        }
    }
}
