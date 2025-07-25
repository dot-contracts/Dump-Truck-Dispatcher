using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.MultiTenancy;

namespace DispatcherWeb.CustomerNotifications
{
    [Table("CustomerNotificationTenant")]
    public class CustomerNotificationTenant : Entity, ISoftDelete
    {
        public bool IsDeleted { get; set; }

        public int CustomerNotificationId { get; set; }
        public virtual CustomerNotification CustomerNotification { get; set; }

        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
    }
}
