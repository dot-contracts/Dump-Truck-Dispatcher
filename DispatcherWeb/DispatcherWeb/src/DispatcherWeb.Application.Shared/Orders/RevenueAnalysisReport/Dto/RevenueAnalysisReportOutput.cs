using System.Collections.Generic;

namespace DispatcherWeb.Orders.RevenueAnalysisReport.Dto
{
    public class RevenueAnalysisReportOutput
    {
        public RevenueAnalysisReportOutput(List<RevenueAnalysisReportDataItem> revenueAnalysisGraphData)
        {
            RevenueAnalysisGraphData = revenueAnalysisGraphData;
        }

        public List<RevenueAnalysisReportDataItem> RevenueAnalysisGraphData { get; set; }
    }
}
