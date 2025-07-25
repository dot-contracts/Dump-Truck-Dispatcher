using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Emailing
{
    public class TrackableEmailEvent : CreationAuditedEntity
    {
        private static class FieldLengthes
        {
            public const int Event = 40;
            public const int SendGridEventId = 100;
            public const int FailReason = 1000;
            public const int Email = 256;
        }

        [StringLength(FieldLengthes.Event)]
        public string Event { get; set; }

        public long? SendGridEventTimestamp { get; set; }

        [StringLength(FieldLengthes.SendGridEventId)]
        public string SendGridEventId { get; set; }

        [StringLength(FieldLengthes.FailReason)]
        public string FailReason { get; set; }

        [StringLength(FieldLengthes.Email)]
        public string Email { get; set; }

        public EmailDeliveryStatus? EmailDeliveryStatus { get; set; }

        public Guid? TrackableEmailId { get; set; }

        public int? TrackableEmailReceiverId { get; set; }

        public virtual TrackableEmail TrackableEmail { get; set; }

        public virtual TrackableEmailReceiver TrackableEmailReceiver { get; set; }

        public void TruncateFieldsIfNeeded()
        {
            if (Event?.Length > FieldLengthes.Event)
            {
                Event = Event.Substring(0, FieldLengthes.Event);
            }

            if (SendGridEventId?.Length > FieldLengthes.SendGridEventId)
            {
                SendGridEventId = SendGridEventId.Substring(0, FieldLengthes.SendGridEventId);
            }

            if (FailReason?.Length > FieldLengthes.FailReason)
            {
                FailReason = FailReason.Substring(0, FieldLengthes.FailReason);
            }

            if (Email?.Length > FieldLengthes.Email)
            {
                Email = Email.Substring(0, FieldLengthes.Email);
            }
        }
    }
}
