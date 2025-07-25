using System;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto
{
    public class RevenueBreakdownByTruckReportInput
    {
        public int? OfficeId { get; set; }

        public DateTime DeliveryDateBegin { get; set; }
        public DateTime DeliveryDateEnd { get; set; }

        public Shift[] Shifts { get; set; }

        public int[] TruckIds { get; set; }
    }
}
