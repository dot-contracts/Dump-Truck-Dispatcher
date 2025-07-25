using System;

namespace DispatcherWeb.Imports.RowReaders
{
    public interface IImportFuelUsageRow : IImportRow
    {
        DateTime? FuelDateTime { get; }
        decimal? Amount { get; }
        decimal? FuelRate { get; }
        decimal? Odometer { get; }
    }
}
