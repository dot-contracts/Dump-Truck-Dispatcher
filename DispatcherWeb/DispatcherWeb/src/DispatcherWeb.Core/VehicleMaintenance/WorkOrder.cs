using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Attributes;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.VehicleMaintenance
{
    [Table("WorkOrder")]
    public class WorkOrder : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int? VehicleServiceTypeId { get; set; }
        public VehicleServiceType VehicleServiceType { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public WorkOrderStatus Status { get; set; }

        public int TruckId { get; set; }
        public Truck Truck { get; set; }

        [StringLength(EntityStringFieldLengths.WorkOrder.Note)]
        public string Note { get; set; }

        [MileageColumn]
        public decimal Odometer { get; set; }

        public decimal Hours { get; set; }

        public long? AssignedToId { get; set; }
        public User AssignedTo { get; set; }

        public decimal TotalLaborCost { get; set; }
        public bool IsTotalLaborCostOverridden { get; set; }
        public decimal TotalPartsCost { get; set; }
        public bool IsTotalPartsCostOverridden { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalCost { get; set; }

        public ICollection<WorkOrderLine> WorkOrderLines { get; set; }
        public ICollection<WorkOrderPicture> WorkOrderPictures { get; set; }
    }
}
