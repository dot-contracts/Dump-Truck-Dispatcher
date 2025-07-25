using CsvHelper;
using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.WorkOrders.VehicleWorkOrderCostReport
{
    public class VehicleWorkOrderCostTableCsv : TableCsvBase, IVehicleWorkOrderCostTable
    {
        public VehicleWorkOrderCostTableCsv(CsvWriter csv) : base(csv)
        {
        }

        public void AddRow(
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
        )
        {
            _csv.WriteField(office);
            _csv.WriteField(vehicle);
            _csv.WriteField(description);
            _csv.WriteField(completionDate);
            _csv.WriteField(workOrderNbr);
            _csv.WriteField(serviceName);
            _csv.WriteField(note);
            _csv.WriteField(laborCost);
            _csv.WriteField(partsCost);
            _csv.WriteField(tax);
            _csv.WriteField(discount);
            _csv.WriteField(totalCost);

            _csv.NextRecord();
        }
    }
}
