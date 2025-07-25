using Abp.Domain.Entities;
using DispatcherWeb.MultiTenancy;

namespace DispatcherWeb.HostEmails
{
    public class HostEmailTenant : Entity, ISoftDelete
    {
        public bool IsDeleted { get; set; }

        public int HostEmailId { get; set; }
        public virtual HostEmail HostEmail { get; set; }

        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
    }
}
