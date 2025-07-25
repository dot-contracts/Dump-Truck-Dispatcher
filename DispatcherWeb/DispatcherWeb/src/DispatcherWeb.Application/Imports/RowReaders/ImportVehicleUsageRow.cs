using System;
using System.Linq;
using CsvHelper;
using DispatcherWeb.Imports.Columns;

namespace DispatcherWeb.Imports.RowReaders
{
    public class ImportVehicleUsageRow : TruckRelatedImportRow, IImportVehicleUsageRow
    {
        public ImportVehicleUsageRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public DateTime? ReadingDateTime => GetDate(VehicleUsageColumn.ReadingDateTime, true);
        public decimal? OdometerReading => GetDecimal(VehicleUsageColumn.OdometerReading);
        public decimal? EngineHours => GetDecimal(VehicleUsageColumn.EngineHours);
    }
}
