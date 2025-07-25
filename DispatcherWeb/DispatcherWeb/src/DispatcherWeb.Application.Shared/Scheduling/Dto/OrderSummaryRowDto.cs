using System;

namespace DispatcherWeb.Scheduling.Dto
{
    public class OrderSummaryRowDto
    {
        public double? NumberOfTrucks { get; set; }
        public decimal Utilization { get; set; }
        public decimal MaxUtilization => NumberOfTrucks.HasValue ? Convert.ToDecimal(NumberOfTrucks.Value) : 0;
    }
}
