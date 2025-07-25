using System;
using DispatcherWeb.Infrastructure.Extensions;

namespace DispatcherWeb.Dashboard.RevenueGraph.Dto
{
    public class RevenueGraphData
    {
        public decimal RevenueValue => FreightRevenueValue + MaterialRevenueValue + FuelSurchargeValue;
        public decimal FreightRevenueValue { get; set; }
        public decimal MaterialRevenueValue { get; set; }
        public decimal FuelSurchargeValue => InternalTrucksFuelSurchargeValue + LeaseHaulersFuelSurchargeValue;
        public decimal InternalTrucksFuelSurchargeValue { get; set; }
        public decimal LeaseHaulersFuelSurchargeValue { get; set; }
        public string Period { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public void DivideRevenueBy(decimal? divisor)
        {
            if (divisor == 0)
            {
                divisor = null;
            }

            FreightRevenueValue = (FreightRevenueValue / divisor)?.RoundTo(2) ?? 0;
            MaterialRevenueValue = (MaterialRevenueValue / divisor)?.RoundTo(2) ?? 0;
            InternalTrucksFuelSurchargeValue = (InternalTrucksFuelSurchargeValue / divisor)?.RoundTo(2) ?? 0;
            LeaseHaulersFuelSurchargeValue = (LeaseHaulersFuelSurchargeValue / divisor)?.RoundTo(2) ?? 0;
        }
    }
}
