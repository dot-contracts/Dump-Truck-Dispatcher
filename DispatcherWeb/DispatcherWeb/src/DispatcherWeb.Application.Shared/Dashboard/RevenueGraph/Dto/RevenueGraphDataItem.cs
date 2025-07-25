using System;

namespace DispatcherWeb.Dashboard.RevenueGraph.Dto
{
    public class RevenueGraphDataItem
    {
        public DateTime? DeliveryDate { get; set; }
        public decimal? FreightPricePerUnit { get; set; }
        public decimal? MaterialPricePerUnit { get; set; }
        public virtual decimal MaterialQuantity { get; set; }
        public virtual decimal FreightQuantity { get; set; }
        public int? OrderLineId { get; set; }
        public decimal FreightPriceOriginal { get; set; }
        public decimal MaterialPriceOriginal { get; set; }
        public bool IsFreightPriceOverridden { get; set; }
        public bool IsMaterialPriceOverridden { get; set; }
        public int? CarrierId { get; set; }
        public decimal FuelSurcharge { get; set; }
        public decimal InternalTruckFuelSurcharge => CarrierId == null ? FuelSurcharge : 0;
        public decimal LeaseHaulerFuelSurcharge => CarrierId != null ? FuelSurcharge : 0;
        public string CustomerName { get; set; }
        public string TruckCode { get; set; }
        public string DriverName { get; set; }
    }
}
