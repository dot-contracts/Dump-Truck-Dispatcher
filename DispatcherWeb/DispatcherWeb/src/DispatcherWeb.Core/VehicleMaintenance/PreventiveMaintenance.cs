using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure.Attributes;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.VehicleMaintenance
{
    [Table("PreventiveMaintenance")]
    public class PreventiveMaintenance : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int TruckId { get; set; }
        public Truck Truck { get; set; }

        public int VehicleServiceId { get; set; }
        public VehicleService VehicleService { get; set; }

        public DateTime LastDate { get; set; }

        [MileageColumn]
        public decimal LastMileage { get; set; }

        public decimal LastHour { get; set; }

        public DateTime? DueDate { get; set; }

        [MileageColumn]
        public decimal? DueMileage { get; set; }

        public decimal? DueHour { get; set; }

        public DateTime? WarningDate { get; set; }

        [MileageColumn]
        public decimal? WarningMileage { get; set; }

        public decimal? WarningHour { get; set; }

    }
}
