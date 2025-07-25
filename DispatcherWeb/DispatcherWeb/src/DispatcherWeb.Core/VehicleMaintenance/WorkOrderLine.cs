using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.VehicleMaintenance
{
    [Table("WorkOrderLine")]
    public class WorkOrderLine : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; }

        public int VehicleServiceId { get; set; }
        public VehicleService VehicleService { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }
        public decimal? LaborTime { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? LaborRate { get; set; }
        public decimal? PartsCost { get; set; }
    }
}
