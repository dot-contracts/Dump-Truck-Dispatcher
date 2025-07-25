using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Orders;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Drivers
{
    [Table("EmployeeTime")]
    public class EmployeeTime : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public Guid? Guid { get; set; }

        public long UserId { get; set; }

        public virtual User User { get; set; }

        public int? DriverId { get; set; }

        public virtual Driver Driver { get; set; }

        public int? OrderLineId { get; set; }

        public virtual OrderLine OrderLine { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public decimal? ManualHourAmount { get; set; }

        public decimal? PayRate { get; set; }

        public int TimeClassificationId { get; set; }

        public bool IsImported { get; set; }

        [ForeignKey("Truck")]
        public int? EquipmentId { get; set; }

        public Truck Truck { get; set; }

        public virtual TimeClassification TimeClassification { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        [StringLength(EntityStringFieldLengths.EmployeeTime.Description)]
        public string Description { get; set; }

        public virtual EmployeeTimePayStatementTime PayStatementTime { get; set; }

        public int? TimeOffId { get; set; }

        public virtual TimeOff TimeOff { get; set; }
    }
}
