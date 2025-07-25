using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Emailing
{
    public class TrackableEmail : AuditedEntity<Guid>, IMayHaveTenant
    {
        public TrackableEmail()
        {
            Events = new HashSet<TrackableEmailEvent>();
            Receivers = new HashSet<TrackableEmailReceiver>();
            QuoteEmails = new HashSet<QuoteEmail>();
            OrderEmails = new HashSet<OrderEmail>();
            InvoiceEmails = new HashSet<InvoiceEmail>();
        }

        public int? TenantId { get; set; }

        [StringLength(EntityStringFieldLengths.TrackableEmail.Subject)]
        public string Subject { get; set; }

        public EmailDeliveryStatus CalculatedDeliveryStatus { get; set; }

        public virtual ICollection<TrackableEmailEvent> Events { get; set; }

        public virtual ICollection<TrackableEmailReceiver> Receivers { get; set; }

        public virtual ICollection<QuoteEmail> QuoteEmails { get; set; }

        public virtual ICollection<OrderEmail> OrderEmails { get; set; }

        public virtual ICollection<InvoiceEmail> InvoiceEmails { get; set; }

        public virtual User CreatorUser { get; set; }

        public void TruncateFieldsIfNeeded()
        {
            if (Subject?.Length > EntityStringFieldLengths.TrackableEmail.Subject)
            {
                Subject = Subject.Substring(0, EntityStringFieldLengths.TrackableEmail.Subject);
            }
        }

    }
}
