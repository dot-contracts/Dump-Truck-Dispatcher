using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Drivers.Dto;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.EmployeeTime.Dto
{
    public class EmployeeTimeEditDto
    {
        public int? Id { get; set; }

        public bool LockToCurrentUser { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        public int? DriverId { get; set; }

        public string DriverCompany { get; set; }

        public bool HasSingleDriverCompany { get; set; }

        public string EmployeeName { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public decimal? ManualHourAmount { get; set; }

        public decimal? PayRate { get; set; }

        [Required]
        public int TimeClassificationId { get; set; }

        public string TimeClassificationName { get; set; }

        public bool TimeClassificationAllowsManualTime { get; set; }

        [StringLength(EntityStringFieldLengths.EmployeeTime.Description)]
        public string Description { get; set; }

        public int? TimeOffId { get; set; }

        public List<DriverCompanyDto> DriverCompanies { get; set; }
    }
}
