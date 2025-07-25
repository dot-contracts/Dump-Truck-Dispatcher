using System;

namespace DispatcherWeb.Orders.RevenueAnalysisReport.Dto
{
    public class RevenueAnalysisReportInput
    {
        public DateTime DeliveryDateBegin { get; set; }
        public DateTime DeliveryDateEnd { get; set; }
        public AnalyzeRevenueBy AnalyzeBy { get; set; }
        public int? OfficeId { get; set; }
        public bool? HasLeaseHaulerId { get; set; }
    }
}
