using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.CustomerNotifications
{
    [Table("CustomerNotificationRole")]
    public class CustomerNotificationRole : Entity, ISoftDelete
    {
        public bool IsDeleted { get; set; }

        public int CustomerNotificationId { get; set; }
        public virtual CustomerNotification CustomerNotification { get; set; }

        [StringLength(EntityStringFieldLengths.CustomerNotificationRole.RoleName)]
        public string RoleName { get; set; }
    }
}
