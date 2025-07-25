using System;

namespace DispatcherWeb.Orders.RevenueBreakdownReport.Dto
{
    public class FillDriversTimeCallbackArgs
    {
        public int EmployeeTimeId { get; set; }
        public int? TruckId { get; set; }
        public long UserId { get; set; }
        public int? DriverId { get; set; }
        public int? OrderLineId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal HoursToAdd { get; set; }
        public int TimeClassificationId { get; set; }
        public bool IsProductionBased { get; set; }
        public decimal? PayRate { get; set; }
    }
}
