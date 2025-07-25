using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.WorkOrders.Dto
{
    public class WorkOrderLineEditDto
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int VehicleServiceId { get; set; }
        public string VehicleServiceName { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }
        public decimal? LaborTime { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? LaborRate { get; set; }
        public decimal? PartsCost { get; set; }
    }
}
