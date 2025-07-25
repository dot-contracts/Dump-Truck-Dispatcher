using System;

namespace DispatcherWeb.Orders.RevenueBreakdownReport.Dto
{
    public class RevenueBreakdownReportInput
    {
        public int? CustomerId { get; set; }
        public int? OfficeId { get; set; }

        public DateTime DeliveryDateBegin { get; set; }
        public DateTime DeliveryDateEnd { get; set; }

        public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

        public int? ItemId { get; set; }

        public Shift[] Shifts { get; set; }
    }
}
