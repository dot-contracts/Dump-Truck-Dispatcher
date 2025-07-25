using System;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class LeaseHaulerRequestDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Shift { get; set; }
        public string LeaseHauler { get; set; }
        public DateTime? Sent { get; set; }
        public string Message { get; set; }
        public string Comments { get; set; }
        public int? NumberTrucksRequested { get; set; }
        public int? Available { get; set; }
        public int? Approved { get; set; }
        public int? Scheduled { get; set; }
        public int? SpecifiedTrucks { get; set; }
    }
}
