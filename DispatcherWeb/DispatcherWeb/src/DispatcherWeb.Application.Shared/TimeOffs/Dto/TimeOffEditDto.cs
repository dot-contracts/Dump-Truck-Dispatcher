using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.TimeOffs.Dto
{
    public class TimeOffEditDto
    {
        public int? Id { get; set; }

        [Required]
        public int DriverId { get; set; }

        public long? EmployeeId { get; set; }

        public string DriverName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal? RequestedHours { get; set; }

        public string Reason { get; set; }

    }
}
