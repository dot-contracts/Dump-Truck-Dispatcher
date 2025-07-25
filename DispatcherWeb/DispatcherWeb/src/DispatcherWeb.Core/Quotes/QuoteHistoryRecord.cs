using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.Quotes
{
    [Table("QuoteHistory")]
    public class QuoteHistoryRecord : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int QuoteId { get; set; }

        public DateTime DateTime { get; set; }

        public QuoteChangeType ChangeType { get; set; }

        public virtual ICollection<QuoteFieldDiff> FieldDiffs { get; set; }

        public virtual Quote Quote { get; set; }

        public virtual User CreatorUser { get; set; }
    }
}
