using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.WebPush;

namespace DispatcherWeb.Drivers
{
    [Table("DriverPushSubscription")]
    public class DriverPushSubscription : FullAuditedEntity
    {
        public int DriverId { get; set; }
        public int? DeviceId { get; set; }
        public int PushSubscriptionId { get; set; }
        public virtual Driver Driver { get; set; }
        public virtual DriverApplicationDevice Device { get; set; }
        public virtual PushSubscription PushSubscription { get; set; }
    }
}
