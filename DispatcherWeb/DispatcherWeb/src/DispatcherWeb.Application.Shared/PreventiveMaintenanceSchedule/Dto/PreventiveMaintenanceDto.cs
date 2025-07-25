using System;

namespace DispatcherWeb.PreventiveMaintenanceSchedule.Dto
{
    public class PreventiveMaintenanceDto
    {
        public int Id { get; set; }
        public string TruckCode { get; set; }
        public decimal CurrentMileage { get; set; }
        public string VehicleServiceName { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? DueMileage { get; set; }
        public DateTime? WarningDate { get; set; }
        public decimal? WarningMileage { get; set; }

        public int? DaysUntilDue { get; set; }
        public decimal? MilesUntilDue { get; set; }
    }
}
