using System;

namespace DispatcherWeb.VehicleUsages.Dto
{
    public class VehicleUsageEditDto
    {
        public int Id { get; set; }
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public DateTime ReadingDateTime { get; set; }
        public ReadingType ReadingType { get; set; }
        public decimal Reading { get; set; }
    }
}
