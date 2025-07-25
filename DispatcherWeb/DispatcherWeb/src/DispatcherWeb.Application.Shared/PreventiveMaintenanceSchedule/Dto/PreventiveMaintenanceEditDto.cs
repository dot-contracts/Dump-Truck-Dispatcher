using System;

namespace DispatcherWeb.PreventiveMaintenanceSchedule.Dto
{
    public class PreventiveMaintenanceEditDto
    {
        public int Id { get; set; }

        public int TruckId { get; set; }
        public string TruckCode { get; set; }

        public int VehicleServiceId { get; set; }
        public string VehicleServiceName { get; set; }

        public DateTime? LastDate { get; set; }
        public decimal LastMileage { get; set; }

        public DateTime? DueDate { get; set; }
        public decimal? DueMileage { get; set; }

        public DateTime? WarningDate { get; set; }
        public decimal? WarningMileage { get; set; }

        public decimal? DueHour { get; set; }
        public decimal? WarningHour { get; set; }
        public decimal LastHour { get; set; }
    }
}
