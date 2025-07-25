using System;

namespace DispatcherWeb.Orders.RevenueBreakdownReport.Dto
{
    public class RevenueBreakdownItem
    {
        public string Customer { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string LoadAtName { get; set; }
        public string DeliverToName { get; set; }
        public string Item { get; set; }
        public string MaterialUom { get; set; }
        public string FreightUom { get; set; }
        public decimal? MaterialRate { get; set; }
        public decimal? FreightRate { get; set; }
        public decimal? DriverPayRate { get; set; }
        public decimal? PlannedMaterialQuantity { get; set; }
        public decimal? PlannedFreightQuantity { get; set; }
        public virtual decimal? ActualMaterialQuantity { get; set; }
        public virtual decimal? ActualFreightQuantity { get; set; }
        public decimal MaterialRevenue => IsMaterialPriceOverridden
            ? MaterialPriceOriginal
            : decimal.Round((MaterialRate ?? 0) * (ActualMaterialQuantity ?? 0), 2);
        public decimal FreightRevenue => IsFreightPriceOverridden
            ? FreightPriceOriginal
            : decimal.Round((FreightRate ?? 0) * (ActualFreightQuantity ?? 0), 2);
        public decimal MaterialPriceOriginal { get; set; }
        public decimal FreightPriceOriginal { get; set; }
        public bool IsMaterialPriceOverridden { get; set; }
        public bool IsFreightPriceOverridden { get; set; }
        public Shift? Shift { get; set; }
        public decimal DriverTime { get; set; }
        public virtual decimal FuelSurcharge => 0;
        public decimal TotalRevenue => MaterialRevenue + FreightRevenue + FuelSurcharge;
        public decimal? RevenuePerHour => DriverTime == 0 ? (decimal?)null : TotalRevenue / DriverTime;
        public virtual int? TicketCount { get; set; }

        public decimal? PriceOverride
        {
            get
            {
                if (!IsFreightPriceOverridden && !IsMaterialPriceOverridden)
                {
                    return null;
                }

                decimal result = 0;
                if (IsFreightPriceOverridden)
                {
                    result += FreightPriceOriginal;
                }

                if (IsMaterialPriceOverridden)
                {
                    result += MaterialPriceOriginal;
                }

                return result;
            }
        }
    }
}
