using System;

namespace DispatcherWeb.WorkOrders.VehicleWorkOrderCostReport.Dto
{
    public class VehicleWorkOrderCostItem
    {
        public int Id { get; set; }
        public string Office { get; set; }
        public string Vehicle { get; set; }
        public string Description { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string ServiceName { get; set; }
        public string Note { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalCost { get; set; }
    }
}
