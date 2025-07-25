using System;
using System.Linq;
using CsvHelper;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Imports.RowReaders
{
    public class ImportFuelUsageRow : TruckRelatedImportRow, IImportFuelUsageRow
    {
        public ImportFuelUsageRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public DateTime? FuelDateTime => GetDate(FuelUsageColumn.FuelDateTime, true);
        public decimal? Amount => GetDecimal(FuelUsageColumn.Amount, true);
        public decimal? FuelRate => GetDecimal(FuelUsageColumn.FuelRate);
        public decimal? Odometer => GetDecimal(FuelUsageColumn.Odometer, 1);
        public string TicketNumber => GetString(FuelUsageColumn.TicketNumber, EntityStringFieldLengths.Ticket.TicketNumber);
    }
}
