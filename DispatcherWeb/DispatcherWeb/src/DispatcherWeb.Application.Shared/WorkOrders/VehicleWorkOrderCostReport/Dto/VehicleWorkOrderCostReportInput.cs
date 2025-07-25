using System;

namespace DispatcherWeb.WorkOrders.VehicleWorkOrderCostReport.Dto
{
    public class VehicleWorkOrderCostReportInput
    {
        public DateTime? IssueDateBegin { get; set; }
        public DateTime? IssueDateEnd { get; set; }

        public DateTime? StartDateBegin { get; set; }
        public DateTime? StartDateEnd { get; set; }

        public DateTime? CompletionDateBegin { get; set; }
        public DateTime? CompletionDateEnd { get; set; }

        public int? TruckId { get; set; }
        public long? AssignedToId { get; set; }

        public WorkOrderStatus? Status { get; set; }

        public int[] OfficeIds { get; set; }
    }
}
