using System;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class RemoveAvailableLeaseHaulerTruckFromScheduleInput
    {
        public int TruckId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public int? OfficeId { get; set; }
    }
}
