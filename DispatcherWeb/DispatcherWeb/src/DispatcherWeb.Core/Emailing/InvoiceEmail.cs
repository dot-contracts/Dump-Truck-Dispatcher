using System;
using Abp.Domain.Entities;
using DispatcherWeb.Invoices;

namespace DispatcherWeb.Emailing
{
    public class InvoiceEmail : Entity, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public int InvoiceId { get; set; }
        public Guid EmailId { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual TrackableEmail Email { get; set; }
    }
}
