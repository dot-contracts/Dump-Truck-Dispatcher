using System;

namespace DispatcherWeb.Tickets.JobsMissingTicketsReport.Dto
{
    public class JobsMissingTicketsReportInput
    {
        public DateTime DeliveryDateBegin { get; set; }
        public DateTime DeliveryDateEnd { get; set; }
    }
}