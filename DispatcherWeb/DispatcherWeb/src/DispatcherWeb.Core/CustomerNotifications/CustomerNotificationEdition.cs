using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Editions;
using Abp.Domain.Entities;

namespace DispatcherWeb.CustomerNotifications
{
    [Table("CustomerNotificationEdition")]
    public class CustomerNotificationEdition : Entity, ISoftDelete
    {
        public bool IsDeleted { get; set; }

        public int CustomerNotificationId { get; set; }
        public virtual CustomerNotification CustomerNotification { get; set; }

        public int EditionId { get; set; }
        public virtual Edition Edition { get; set; }
    }
}
