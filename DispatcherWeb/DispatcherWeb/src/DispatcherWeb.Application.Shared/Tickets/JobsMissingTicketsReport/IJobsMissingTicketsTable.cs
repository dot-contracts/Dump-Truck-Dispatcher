using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Tickets.JobsMissingTicketsReport
{
    public interface IJobsMissingTicketsTable : IAddColumnHeaders
    {
        void AddRow(
            string customer,
            string item,
            string deliveryDate,
            string orderId,
            string deliverTo,
            string truck,
            string driver);
    }
}