using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Orders.RevenueBreakdownReport
{
    public interface IRevenueBreakdownTable : IAddColumnHeaders
    {
        void AddRow(
            string customer,
            string deliveryDate,
            string shiftName,
            string loadAt,
            string deliverTo,
            string item,
            string materialUom,
            string freightUom,
            string materialRate,
            string freightRate,
            string driverPayRate,
            string plannedMaterialQuantity,
            string plannedFreightQuantity,
            string actualMaterialQuantity,
            string actualFreightQuantity,
            string freightRevenue,
            string materialRevenue,
            string fuelSurcharge,
            string totalRevenue,
            string driverTime,
            string revenuePerHour,
            string ticketCount,
            string priceOverride
        );

    }
}
