using System;
using System.Linq;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverActivityDetailReportLoadDto : ITicketQuantity
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }
        public int DispatchId { get; set; }
        public int OrderLineId { get; set; }
        public string CustomerName { get; set; }
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }
        public string LoadTicket { get; set; }
        public decimal? FreightQuantityOrdered { get; set; }
        public decimal? MaterialQuantityOrdered { get; set; }
        public decimal? FreightQuantity { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public DesignationEnum Designation { get; set; }
        public int? OrderLineMaterialUomId { get; set; }
        public int? OrderLineFreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public string FreightUomName { get; set; }
        public string MaterialUomName { get; set; }
        public string TrailerTruckCode { get; set; }
        public string VehicleCategory { get; set; }
        public DateTime? LoadTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public TimeSpan? CycleTime { get; set; }
        public string JobNumber { get; set; }
        public string FreightItemName { get; set; }
        public string MaterialItemName { get; set; }

        public string ItemName
        {
            get
            {
                var items = new[] { FreightItemName, MaterialItemName };
                return string.Join(" / ", items.Where(x => !string.IsNullOrEmpty(x)));
            }
        }

        public decimal? QuantityOrdered
        {
            get
            {
                var useMaterial = this.GetAmountTypeToUse().useMaterial;
                if (useMaterial)
                {
                    return MaterialQuantityOrdered;
                }
                else
                {
                    return FreightQuantityOrdered;
                }
            }
        }

        public decimal? QuantityDelivered
        {
            get
            {
                var useMaterial = this.GetAmountTypeToUse().useMaterial;
                if (useMaterial)
                {
                    return MaterialQuantity ?? 0;
                }
                else
                {
                    return FreightQuantity ?? 0;
                }
            }
        }
    }
}
