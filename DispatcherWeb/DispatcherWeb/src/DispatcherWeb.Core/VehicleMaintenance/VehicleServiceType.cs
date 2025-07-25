using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.VehicleMaintenance
{
    [Table("VehicleServiceType")]
    public class VehicleServiceType : FullAuditedEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<WorkOrder> WorkOrders { get; set; }
    }
}
