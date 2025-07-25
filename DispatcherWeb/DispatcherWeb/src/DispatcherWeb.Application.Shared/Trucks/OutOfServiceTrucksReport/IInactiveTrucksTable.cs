using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Trucks.OutOfServiceTrucksReport
{
    public interface IInactiveTrucksTable : IAddColumnHeaders
    {
        void AddRow(string divisionName, string inactiveTrucks);
    }
}
