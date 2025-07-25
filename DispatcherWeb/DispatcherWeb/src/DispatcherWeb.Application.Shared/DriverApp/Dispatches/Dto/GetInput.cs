using System;
using DispatcherWeb.Dto;

namespace DispatcherWeb.DriverApp.Dispatches.Dto
{
    public class GetInput : PagedInputDto
    {
        public int? Id { get; set; }

        public int? TruckId { get; set; }

        public DateTime? OrderDateBegin { get; set; }

        public DateTime? OrderDateEnd { get; set; }

        public DateTime? ModifiedAfterDateTime { get; set; }

        public DispatchStatus[] DispatchStatuses { get; set; }

        public bool? HasLoads { get; set; }
    }
}
