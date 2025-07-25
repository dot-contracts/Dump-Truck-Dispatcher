using System;
using System.Collections.Generic;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class LeaseHaulerRequestEditModel
    {
        public int Id { get; set; }

        public int? OrderLineId { get; set; }

        public DateTime Date { get; set; }

        public Shift? Shift { get; set; }

        public int OfficeId { get; set; }

        public int LeaseHaulerId { get; set; }

        public int? Available { get; set; }

        public int? Approved { get; set; }

        public int? NumberTrucksRequested { get; set; }
        public string Comments { get; set; }

        public LeaseHaulerRequestStatus? Status { get; set; }

        public bool SuppressLeaseHaulerDispatcherNotification { get; set; }

        public List<int?> Trucks { get; set; }

        public List<int?> Drivers { get; set; }
    }
}
