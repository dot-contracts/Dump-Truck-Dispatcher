using System;

namespace DispatcherWeb.Tickets.JobsMissingTicketsReport.Dto
{
    public class JobsMissingTicket
    {
        public string CustomerName { get; set; }
        public string ItemName { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int OrderId { get; set; }
        public string DeliverTo { get; set; }
        public string TruckCode { get; set; }
        public string Driver { get; set; }
    }
}
