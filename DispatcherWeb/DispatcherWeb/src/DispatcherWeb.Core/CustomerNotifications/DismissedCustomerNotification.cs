using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.CustomerNotifications
{
    [Table("DismissedCustomerNotification")]
    public class DismissedCustomerNotification : Entity, ISoftDelete
    {
        public bool IsDeleted { get; set; }

        public int CustomerNotificationId { get; set; }
        public virtual CustomerNotification CustomerNotification { get; set; }

        public long UserId { get; set; }
        public virtual User User { get; set; }
    }
}
