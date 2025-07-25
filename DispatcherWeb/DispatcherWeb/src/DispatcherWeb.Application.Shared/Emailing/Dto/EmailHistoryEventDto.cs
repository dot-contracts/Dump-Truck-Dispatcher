using System;

namespace DispatcherWeb.Emailing.Dto
{
    public class EmailHistoryEventDto
    {
        public int Id { get; set; }
        public EmailDeliveryStatus? EmailDeliveryStatus { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
