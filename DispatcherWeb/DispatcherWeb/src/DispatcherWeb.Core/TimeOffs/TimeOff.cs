using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.TimeOffs
{
    [Table("TimeOff")]
    public class TimeOff : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int DriverId { get; set; }

        public virtual Driver Driver { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal? RequestedHours { get; set; }

        [StringLength(EntityStringFieldLengths.TimeOff.Reason)]
        public string Reason { get; set; }

        public virtual ICollection<EmployeeTime> EmployeeTimes { get; set; }
    }
}
