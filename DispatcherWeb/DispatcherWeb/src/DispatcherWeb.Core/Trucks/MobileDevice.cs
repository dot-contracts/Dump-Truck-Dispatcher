using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Trucks
{
    [Table("MobileDevice")]
    public class MobileDevice : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int TruckId { get; set; }

        public virtual Truck Truck { get; set; }

        public DeviceTypeEnum DeviceType { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Make)]
        public string Make { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Model)]
        public string Model { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.Imei)]
        public string Imei { get; set; }

        [StringLength(EntityStringFieldLengths.Truck.SimId)]
        public string SimId { get; set; }
    }
}
