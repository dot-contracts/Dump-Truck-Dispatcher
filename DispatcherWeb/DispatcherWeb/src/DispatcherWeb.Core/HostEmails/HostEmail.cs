using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.HostEmails
{
    public class HostEmail : FullAuditedEntity
    {
        [StringLength(EntityStringFieldLengths.HostEmail.Subject)]
        public string Subject { get; set; }

        [StringLength(EntityStringFieldLengths.HostEmail.Body)]
        public string Body { get; set; }

        public virtual ICollection<HostEmailEdition> Editions { get; set; }

        public bool? ActiveFilter { get; set; }

        public virtual ICollection<HostEmailTenant> Tenants { get; set; }

        public HostEmailType Type { get; set; }

        public virtual ICollection<HostEmailRole> Roles { get; set; }

        public virtual ICollection<HostEmailReceiver> Receivers { get; set; }

        public virtual User CreatorUser { get; set; }

        public DateTime? ProcessedAtDateTime { get; set; }
    }
}
