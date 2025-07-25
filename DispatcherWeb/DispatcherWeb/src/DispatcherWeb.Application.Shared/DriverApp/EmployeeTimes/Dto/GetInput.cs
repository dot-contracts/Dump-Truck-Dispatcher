using System;
using DispatcherWeb.Dto;

namespace DispatcherWeb.DriverApp.EmployeeTimes.Dto
{
    public class GetInput : PagedInputDto
    {
        public int? Id { get; set; }

        public int? TruckId { get; set; }

        public DateTime? StartDateTimeBegin { get; set; }

        public DateTime? StartDateTimeEnd { get; set; }

        public DateTime? EndDateTimeBegin { get; set; }

        public DateTime? EndDateTimeEnd { get; set; }
        public bool? HasEndTime { get; set; }
        public bool? IsImported { get; set; }

        public DateTime? ModifiedAfterDateTime { get; set; }
    }
}
