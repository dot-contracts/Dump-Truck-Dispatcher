using System;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class GetLeaseHaulerRequestForEditInput
    {
        public int? OrderLineId { get; set; }

        public int? LeaseHaulerRequestId { get; set; }

        public DateTime? Date { get; set; }

        public bool SuppressLeaseHaulerDispatcherNotification { get; set; }
    }
}
