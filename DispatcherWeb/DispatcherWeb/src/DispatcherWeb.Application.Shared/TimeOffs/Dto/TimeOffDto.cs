using System;

namespace DispatcherWeb.TimeOffs.Dto
{
    public class TimeOffDto
    {
        public int Id { get; set; }
        public string DriverName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? RequestedHours { get; set; }
        public string Reason { get; set; }
        public decimal? PaidHours { get; set; }
    }
}
