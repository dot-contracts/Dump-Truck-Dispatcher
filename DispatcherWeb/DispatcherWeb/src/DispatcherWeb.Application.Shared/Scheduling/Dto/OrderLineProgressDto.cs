using System;
using System.Collections.Generic;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.Scheduling.Dto
{
    public class OrderLineProgressDto
    {
        public int Id { get; set; }
        public int DispatchCount { get; set; }
        public List<TicketQuantityDto> Tickets { get; set; }
        public List<LoadDto> Loads { get; set; }

        public class LoadDto
        {
            public DateTime? DestinationDateTime { get; set; }
            public DateTime? SourceDateTime { get; set; }
            public DispatchDto Dispatch { get; set; }
            public List<TicketQuantityDto> Tickets { get; set; }
        }

        public class DispatchDto
        {
            public int Id { get; set; }
            public DateTime? Acknowledged { get; set; }
            public TruckDto Truck { get; set; }
        }

        public class TruckDto
        {
            public int Id { get; set; }
            public decimal? CargoCapacityTons { get; set; }
            public decimal? CargoCapacityCyds { get; set; }
            public string TruckCode { get; set; }
        }
    }
}
