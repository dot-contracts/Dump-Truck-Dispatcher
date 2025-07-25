using DispatcherWeb.Imports;
using DispatcherWeb.Imports.Columns;

namespace DispatcherWeb.Tests.Imports
{
    public static class FieldMapping
    {
        public static FieldMapItem[] FuelUsage { get; }
        public static FieldMapItem[] FuelUsageFromJacobusEnergy { get; }

        public static FieldMapItem[] VehicleUsage { get; }

        static FieldMapping()
        {
            FuelUsage = new[]
            {
                new FieldMapItem { StandardField = FuelUsageColumn.Office, UserField = FuelUsageColumn.Office},
                new FieldMapItem { StandardField = FuelUsageColumn.TruckNumber, UserField = FuelUsageColumn.TruckNumber},
                new FieldMapItem { StandardField = FuelUsageColumn.FuelDateTime, UserField = FuelUsageColumn.FuelDateTime},
                new FieldMapItem { StandardField = FuelUsageColumn.Amount, UserField = FuelUsageColumn.Amount},
                new FieldMapItem { StandardField = FuelUsageColumn.FuelRate, UserField = FuelUsageColumn.FuelRate},
                new FieldMapItem { StandardField = FuelUsageColumn.Odometer, UserField = FuelUsageColumn.Odometer},
                new FieldMapItem { StandardField = FuelUsageColumn.TicketNumber, UserField = FuelUsageColumn.TicketNumber},
            };

            FuelUsageFromJacobusEnergy = new[]
            {
                new FieldMapItem { StandardField = FuelUsageColumn.Office, UserField = FuelUsageFromJacobusEnergyColumn.Office},
                new FieldMapItem { StandardField = FuelUsageColumn.TruckNumber, UserField = FuelUsageFromJacobusEnergyColumn.TruckNumber},
                new FieldMapItem { StandardField = FuelUsageColumn.FuelDateTime, UserField = FuelUsageFromJacobusEnergyColumn.FuelDateTime},
                new FieldMapItem { StandardField = FuelUsageColumn.Amount, UserField = FuelUsageFromJacobusEnergyColumn.Amount},
                new FieldMapItem { StandardField = FuelUsageColumn.FuelRate, UserField = FuelUsageFromJacobusEnergyColumn.FuelRate},
                new FieldMapItem { StandardField = FuelUsageColumn.Odometer, UserField = FuelUsageFromJacobusEnergyColumn.Odometer},
                new FieldMapItem { StandardField = FuelUsageColumn.TicketNumber, UserField = FuelUsageFromJacobusEnergyColumn.TicketNumber},
            };

            VehicleUsage = new[]
            {
                new FieldMapItem { StandardField = VehicleUsageColumn.Office, UserField = VehicleUsageColumn.Office},
                new FieldMapItem { StandardField = VehicleUsageColumn.TruckNumber, UserField = VehicleUsageColumn.TruckNumber},
                new FieldMapItem { StandardField = VehicleUsageColumn.ReadingDateTime, UserField = VehicleUsageColumn.ReadingDateTime},
                new FieldMapItem { StandardField = VehicleUsageColumn.OdometerReading, UserField = VehicleUsageColumn.OdometerReading},
                new FieldMapItem { StandardField = VehicleUsageColumn.EngineHours, UserField = VehicleUsageColumn.EngineHours},
            };

        }
    }
}
