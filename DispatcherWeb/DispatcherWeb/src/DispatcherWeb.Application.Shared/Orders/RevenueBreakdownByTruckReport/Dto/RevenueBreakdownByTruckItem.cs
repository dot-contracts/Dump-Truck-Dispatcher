using System;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto
{
    public class RevenueBreakdownByTruckItem
    {
        public DateTime? TicketDate { get; set; }
        public Shift? Shift { get; set; }
        public string Truck { get; set; }
        public decimal MaterialRevenue { get; set; }
        public decimal FreightRevenue { get; set; }
        public decimal FuelSurcharge { get; set; }
        public decimal TotalRevenue => MaterialRevenue + FreightRevenue + FuelSurcharge;
        public decimal? RevenuePerHour => DriverTime == 0 ? (decimal?)null : TotalRevenue / DriverTime;
        public int? TruckId { get; set; }
        public decimal DriverTime { get; set; }
    }
}
