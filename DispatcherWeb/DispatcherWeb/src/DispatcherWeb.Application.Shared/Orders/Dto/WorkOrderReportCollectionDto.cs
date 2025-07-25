using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class WorkOrderReportCollectionDto
    {
        public List<WorkOrderReportDto> WorkOrderReports { get; set; }

        public byte[] PaidImageBytes { get; set; }

        public byte[] StaggeredTimeImageBytes { get; set; }

        public bool ConvertPdfTicketImages { get; set; }

        public bool SeparateItems { get; set; }
    }
}
