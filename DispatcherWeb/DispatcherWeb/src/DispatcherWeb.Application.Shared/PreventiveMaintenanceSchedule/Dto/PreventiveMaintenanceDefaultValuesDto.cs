using System;

namespace DispatcherWeb.PreventiveMaintenanceSchedule.Dto
{
    public class PreventiveMaintenanceDefaultValuesDto
    {
        public DateTime? LastDate { get; set; }
        public decimal? LastMileage { get; set; }
        public decimal? LastHour { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? DueMileage { get; set; }
        public decimal? DueHour { get; set; }
        public DateTime? WarningDate { get; set; }
        public decimal? WarningMileage { get; set; }
        public decimal? WarningHour { get; set; }
    }
}
