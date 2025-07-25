using System.Collections.Generic;

namespace DispatcherWeb.JobSummary.Dto
{
    public class OrderTruckDto
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? LoadCount { get; set; }
        public decimal? Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public IList<TripCycleDto> TripCycles { get; set; } = new List<TripCycleDto>();
    }
}


