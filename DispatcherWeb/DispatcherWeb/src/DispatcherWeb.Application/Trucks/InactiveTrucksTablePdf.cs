using DispatcherWeb.Infrastructure.Reports;
using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Trucks.OutOfServiceTrucksReport
{
    public class InactiveTrucksTablePdf : StandardTablePdf, IInactiveTrucksTable
    {
        public InactiveTrucksTablePdf(Section section) : base(section, new[] { 8.0, 10.5 })
        {
        }

        public void AddRow(string divisionName, string inactiveTrucks)
        {
            base.AddRow(divisionName, inactiveTrucks);
        }
    }
}
