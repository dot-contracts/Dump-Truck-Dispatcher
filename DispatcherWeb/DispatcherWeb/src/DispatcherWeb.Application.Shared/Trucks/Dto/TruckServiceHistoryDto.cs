using System;

namespace DispatcherWeb.Trucks.Dto
{
    public class TruckServiceHistoryDto
    {
        public string VehicleServiceName { get; set; }
        public DateTime CompletionDate { get; set; }
        public decimal Mileage { get; set; }
    }
}
