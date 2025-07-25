using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Trucks.OutOfServiceTrucksReport
{
    public interface IOutOfServiceTrucksTable : IAddColumnHeaders
    {
        void AddRow(
            string truckNumber,
            string outOfServiceDate,
            string numberOfDaysOutOfService,
            string reason
        );
    }
}
