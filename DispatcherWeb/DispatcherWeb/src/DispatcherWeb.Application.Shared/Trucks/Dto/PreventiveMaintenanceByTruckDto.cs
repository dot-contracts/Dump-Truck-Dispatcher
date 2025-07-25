using System;

namespace DispatcherWeb.Trucks.Dto
{
    public class PreventiveMaintenanceByTruckDto
    {
        public string VehicleServiceName { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? DueMileage { get; set; }
        public decimal? DueHour { get; set; }
    }
}
