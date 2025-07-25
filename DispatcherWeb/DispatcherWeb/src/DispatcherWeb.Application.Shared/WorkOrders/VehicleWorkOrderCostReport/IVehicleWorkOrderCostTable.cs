using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.WorkOrders.VehicleWorkOrderCostReport
{
    public interface IVehicleWorkOrderCostTable : IAddColumnHeaders
    {
        void AddRow(
            string office,
            string vehicle,
            string description,
            string completionDate,
            string workOrderNbr,
            string serviceName,
            string note,
            string laborCost,
            string partsCost,
            string tax,
            string discount,
            string totalCost
        );
    }
}
