namespace DispatcherWeb.Orders.RevenueAnalysisReport.Dto
{
    public class RevenueAnalysisReportDataItem
    {
        public decimal RevenueValue => FreightRevenueValue + MaterialRevenueValue + FuelSurchargeValue;
        public decimal FreightRevenueValue { get; set; }
        public decimal MaterialRevenueValue { get; set; }
        public decimal FuelSurchargeValue => InternalTrucksFuelSurchargeValue + LeaseHaulersFuelSurchargeValue;
        public decimal InternalTrucksFuelSurchargeValue { get; set; }
        public decimal LeaseHaulersFuelSurchargeValue { get; set; }
        public string AnalysisBy { get; set; }
    }
}
