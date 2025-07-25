using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport
{
    public interface IRevenueBreakdownByTruckTable : IAddColumnHeaders
    {
        void AddRow(
            string deliveryDate,
            string shiftName,
            string truck,
            string freightRevenue,
            string materialRevenue,
            string fuelSurcharge,
            string totalRevenue,
            string driverTime,
            string revenuePerHour
        );

    }
}
