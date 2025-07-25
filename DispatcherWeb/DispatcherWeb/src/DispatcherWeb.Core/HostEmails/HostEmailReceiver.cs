using System;
using Abp.Domain.Entities;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Emailing;
using DispatcherWeb.MultiTenancy;

namespace DispatcherWeb.HostEmails
{
    public class HostEmailReceiver : Entity, ISoftDelete
    {
        public bool IsDeleted { get; set; }

        public int HostEmailId { get; set; }
        public virtual HostEmail HostEmail { get; set; }

        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }

        public long UserId { get; set; }
        public virtual User User { get; set; }

        public Guid? TrackableEmailId { get; set; }
        public virtual TrackableEmail TrackableEmail { get; set; }
    }
}
