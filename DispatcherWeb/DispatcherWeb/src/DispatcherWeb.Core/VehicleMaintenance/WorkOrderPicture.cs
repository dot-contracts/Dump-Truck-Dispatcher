using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.VehicleMaintenance
{
    [Table("WorkOrderPicture")]
    public class WorkOrderPicture : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; }

        public Guid FileId { get; set; }

        [StringLength(EntityStringFieldLengths.WorkOrderPicture.FileName)]
        public string FileName { get; set; }
    }
}
