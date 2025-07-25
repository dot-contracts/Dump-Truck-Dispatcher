using System;
using System.Linq;
using CsvHelper;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Imports.RowReaders
{
    public class TicketEarningsImportRow : ImportRow
    {
        private const string ColumnPrefix = "Haultickets_";

        public TicketEarningsImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public DateTime? TicketDateTime => GetDate(ColumnPrefix + "TicketDateTime", true);
        public string HaulerRef => GetString(ColumnPrefix + "HaulerRef", EntityStringFieldLengths.ImportedEarnings.HaulerRef);
        public string Site => GetString(ColumnPrefix + "Site", EntityStringFieldLengths.ImportedEarnings.Site);
        public string CustomerName => GetString(ColumnPrefix + "CustomerName", EntityStringFieldLengths.ImportedEarnings.CustomerName);
        public string LicensePlate => GetString(ColumnPrefix + "Licenseplate", EntityStringFieldLengths.ImportedEarnings.LicensePlate);
        public decimal? HaulPaymentRate => GetDecimal(ColumnPrefix + "HaulPaymentRate", true);
        public string HaulPaymentRateUom => GetString(ColumnPrefix + "HaulPaymentRateUOM", EntityStringFieldLengths.ImportedEarnings.Uom);
        public decimal? NetTons => GetDecimal(ColumnPrefix + "NetTons", true);
        public decimal? FscAmount => GetDecimal(ColumnPrefix + "FSCAmount", false); //not included in HaulPayment
        public decimal? HaulPayment => GetDecimal(ColumnPrefix + "HaulPayment", true); //HaulPaymentRate * NetTons
        public string ProductDescription => GetString(ColumnPrefix + "ProductDescription", EntityStringFieldLengths.ImportedEarnings.ProductDescription);
        public string TicketNumber => GetString(ColumnPrefix + "TicketID", EntityStringFieldLengths.ImportedEarnings.TicketNumber);
    }
}
