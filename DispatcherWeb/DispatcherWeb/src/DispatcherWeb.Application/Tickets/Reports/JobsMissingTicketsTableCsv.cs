using CsvHelper;
using DispatcherWeb.Infrastructure.Reports;
using DispatcherWeb.Tickets.JobsMissingTicketsReport;

namespace DispatcherWeb.Tickets.Reports;

public class JobsMissingTicketsTableCsv : TableCsvBase, IJobsMissingTicketsTable
{
    public JobsMissingTicketsTableCsv(CsvWriter csv)
        : base(csv)
    {

    }

    public void AddRow(
        string customer,
        string item,
        string deliveryDate,
        string orderId,
        string deliverTo,
        string truck,
        string driver)
    {
        _csv.WriteField(customer);
        _csv.WriteField(item);
        _csv.WriteField(deliveryDate);
        _csv.WriteField(orderId);
        _csv.WriteField(deliverTo);
        _csv.WriteField(truck);
        _csv.WriteField(driver);
        _csv.NextRecord();
    }
}