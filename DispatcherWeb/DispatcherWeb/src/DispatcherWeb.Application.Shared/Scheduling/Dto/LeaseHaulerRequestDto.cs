using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class LeaseHaulerRequestDto
    {
        public int Id { get; set; }
        public int LeaseHaulerId { get; set; }
        public string LeaseHaulerName { get; set; }
        public int? NumberTrucksRequested { get; set; }
        public LeaseHaulerRequestStatus? Status { get; set; }
        public List<ScheduleRequestedLeaseHaulerTruckDto> RequestedTrucks { get; set; }
    }
}
