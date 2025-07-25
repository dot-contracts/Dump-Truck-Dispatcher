using System;

namespace DispatcherWeb.Imports.RowReaders
{
    public interface IImportVehicleUsageRow : IImportRow
    {
        DateTime? ReadingDateTime { get; }
        decimal? OdometerReading { get; }
        decimal? EngineHours { get; }

    }
}
