using System;
using Abp.Timing;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class SendLeaseHaulerRequestDto
    {
        public SendLeaseHaulerRequestDto(SendRequestsInput input, int leaseHaulerId, Guid guid, string message)
        {
            Guid = guid;
            Date = input.Date;
            Shift = input.Shift;
            OfficeId = input.OfficeId;
            LeaseHaulerId = leaseHaulerId;
            Message = message;
            Sent = Clock.Now;
        }

        public Guid Guid { get; }
        public DateTime Date { get; }
        public Shift? Shift { get; }
        public int OfficeId { get; }

        public int LeaseHaulerId { get; }
        public string Message { get; set; }

        public DateTime Sent { get; }
    }
}
