using System;
using System.Collections.Generic;

namespace DispatcherWeb.JobSummary.Dto
{
    public partial class OrderTrucksDto
    {
        public DateTime Earliest { get; set; }
        public DateTime Latest { get; set; }
        public IList<OrderTruckDto> OrderTrucks { get; set; } = new List<OrderTruckDto>();

        public override string ToString()
        {
            return $"Earliest: {Earliest} Latest: {Latest} OrderTrucks: {OrderTrucks.Count}";
        }
    }
}


