using System;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class NotifyLeaseHaulerInput
    {
        public int LeaseHaulerId { get; set; }

        public Guid LeaseHaulerRequestGuid { get; set; }

        public string Message { get; set; }
    }
}
