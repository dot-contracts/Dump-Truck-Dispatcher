using System;

namespace DispatcherWeb.Orders.Dto
{
    public class GetOrderSummaryReportInput
    {
        public DateTime Date { get; set; }
        public int? OfficeId { get; set; }
        public bool HidePrices { get; set; }
    }
}
