using CsvHelper;
using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Orders.RevenueBreakdownByTruckReport
{
    public class RevenueBreakdownByTruckTableCsv : TableCsvBase, IRevenueBreakdownByTruckTable
    {
        public RevenueBreakdownByTruckTableCsv(CsvWriter csv) : base(csv)
        {
        }

        public void AddRow(
            string ticketDate,
            string shiftName,
            string truck,
            string materialRevenue,
            string freightRevenue,
            string fuelSurcharge,
            string totalRevenue,
            string driverTime,
            string revenuePerHour
        )
        {
            _csv.WriteField(ticketDate);
            if (shiftName != null)
            {
                _csv.WriteField(shiftName);
            }
            _csv.WriteField(truck);
            _csv.WriteField(materialRevenue);
            _csv.WriteField(freightRevenue);
            if (fuelSurcharge != null)
            {
                _csv.WriteField(fuelSurcharge);
            }
            _csv.WriteField(totalRevenue);
            _csv.WriteField(driverTime);
            _csv.WriteField(revenuePerHour);

            _csv.NextRecord();
        }
    }
}
