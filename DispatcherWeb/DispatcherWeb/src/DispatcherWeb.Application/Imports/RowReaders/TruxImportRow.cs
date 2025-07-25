using System;
using System.Linq;
using CsvHelper;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Imports.RowReaders
{
    public class TruxImportRow : ImportRow
    {
        public TruxImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public int? JobId => GetInt("Job Id", true);
        public int? ShiftAssignment => GetInt("Shift/Assignment", true);
        public string JobName => GetString("Job Name", EntityStringFieldLengths.TruxEarnings.JobName);
        public DateTime? StartDateTime => GetDateTimeWithTimeZone(" Start Date", true);
        public string TruckType => GetString("Truck Type", EntityStringFieldLengths.TruxEarnings.TruckType);
        public string Status => GetString("Status", EntityStringFieldLengths.TruxEarnings.Status);
        public string TruxTruckId => GetString("Truck Id", EntityStringFieldLengths.TruxEarnings.TruxTruckId);
        public string DriverName => GetString("Driver Name", EntityStringFieldLengths.TruxEarnings.DriverName);
        public string HaulerName => GetString("Hauler Name", EntityStringFieldLengths.TruxEarnings.HaulerName);
        public DateTime? PunchInDatetime => GetDateTimeWithTimeZone("Punch In Datetime", true);
        public DateTime? PunchOutDatetime => GetDateTimeWithTimeZone("Punch Out Datetime", true);
        public decimal? Hours => GetDecimal("Hours", true);
        public decimal? Tons => GetDecimal("Tons", true);
        public int? Loads => GetInt("Loads", true);
        public string Unit => GetString("Unit", EntityStringFieldLengths.TruxEarnings.Unit);
        public decimal? Rate => GetDecimal("Rate", true);
        public decimal? Total => GetDecimal("Total", true);

    }
}
