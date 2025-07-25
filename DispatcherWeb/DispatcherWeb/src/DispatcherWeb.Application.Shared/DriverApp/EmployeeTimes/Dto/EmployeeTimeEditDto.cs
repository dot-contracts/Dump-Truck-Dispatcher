using System;

namespace DispatcherWeb.DriverApp.EmployeeTimes.Dto
{
    public class EmployeeTimeEditDto
    {
        public int Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string Description { get; set; }
        public int? TruckId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? OrderLineId { get; set; }
        public int? TimeClassificationId { get; set; }
    }
}
