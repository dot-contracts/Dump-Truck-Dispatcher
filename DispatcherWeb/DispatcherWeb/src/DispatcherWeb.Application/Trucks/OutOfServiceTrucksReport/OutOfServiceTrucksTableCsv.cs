using CsvHelper;
using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Trucks.OutOfServiceTrucksReport
{
    public class OutOfServiceTrucksTableCsv : TableCsvBase, IOutOfServiceTrucksTable
    {
        public OutOfServiceTrucksTableCsv(CsvWriter csv) : base(csv)
        {
        }

        public void AddRow(
            string truckNumber,
            string outOfServiceDate,
            string numberOfDaysOutOfService,
            string reason
        )
        {
            _csv.WriteField(truckNumber);
            _csv.WriteField(outOfServiceDate);
            _csv.WriteField(numberOfDaysOutOfService);
            _csv.WriteField(reason);

            _csv.NextRecord();
        }
    }
}
