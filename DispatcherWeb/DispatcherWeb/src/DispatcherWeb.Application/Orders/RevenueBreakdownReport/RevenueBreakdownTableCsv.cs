using CsvHelper;
using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Orders.RevenueBreakdownReport
{
    public class RevenueBreakdownTableCsv : TableCsvBase, IRevenueBreakdownTable
    {
        public RevenueBreakdownTableCsv(CsvWriter csv) : base(csv)
        {
        }

        public void AddRow(
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
        )
        {
            _csv.WriteField(customer);
            _csv.WriteField(deliveryDate);
            if (shiftName != null)
            {
                _csv.WriteField(shiftName);
            }
            _csv.WriteField(loadAt);
            _csv.WriteField(deliverTo);
            _csv.WriteField(item);
            _csv.WriteField(materialUom);
            _csv.WriteField(freightUom);
            _csv.WriteField(materialRate);
            _csv.WriteField(freightRate);
            if (driverPayRate != null)
            {
                _csv.WriteField(driverPayRate);
            }
            _csv.WriteField(plannedMaterialQuantity);
            _csv.WriteField(plannedFreightQuantity);
            _csv.WriteField(actualMaterialQuantity);
            _csv.WriteField(actualFreightQuantity);
            _csv.WriteField(freightRevenue);
            _csv.WriteField(materialRevenue);
            if (fuelSurcharge != null)
            {
                _csv.WriteField(fuelSurcharge);
            }
            _csv.WriteField(totalRevenue);
            //_csv.WriteField(driverTime);
            //_csv.WriteField(revenuePerHour);
            _csv.WriteField(ticketCount);
            _csv.WriteField(priceOverride);

            _csv.NextRecord();
        }
    }
}
