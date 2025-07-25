using System;
using DispatcherWeb.Imports.Columns;

namespace DispatcherWeb.Imports
{
    public static class StandardFields
    {
        public static StandardField[] GetFields(ImportType importType)
        {
            switch (importType)
            {
                case ImportType.FuelUsage:
                    return FuelUsageFields;
                case ImportType.VehicleUsage:
                    return VehicleUsageFields;
                case ImportType.Customers:
                    return CustomerFields;
                case ImportType.Vendors:
                    return VendorFields;
                case ImportType.Items:
                    return ItemFields;
                case ImportType.Trucks:
                    return TruckFields;
                case ImportType.Employees:
                    return EmployeeFields;
                case ImportType.Trux:
                    return new StandardField[0];
                case ImportType.LuckStone:
                    return new StandardField[0];
                default:
                    throw new ArgumentOutOfRangeException($"Not supported {nameof(importType)}.", nameof(importType));
            }
        }

        private static StandardField[] FuelUsageFields { get; } =
        {
            new StandardField(null, FuelUsageColumn.Office, isRequired: false),
            new StandardField(null, FuelUsageColumn.TruckNumber, isRequired: true),
            new StandardField(null, FuelUsageColumn.FuelDateTime, isRequired: true),
            new StandardField(null, FuelUsageColumn.Amount, isRequired: true),
            new StandardField(null, FuelUsageColumn.FuelRate, isRequired: false),
            new StandardField(null, FuelUsageColumn.Odometer, isRequired: false),
            new StandardField(null, FuelUsageColumn.TicketNumber, isRequired: false),
        };

        private static StandardField[] VehicleUsageFields { get; } =
        {
            new StandardField(null, VehicleUsageColumn.Office, isRequired: false),
            new StandardField(null, VehicleUsageColumn.TruckNumber, isRequired: true),
            new StandardField(null, VehicleUsageColumn.ReadingDateTime, isRequired: true),
            new StandardField(null, VehicleUsageColumn.OdometerReading, isRequired: true, requireOnlyOneOf: new []{VehicleUsageColumn.EngineHours}),
            new StandardField(null, VehicleUsageColumn.EngineHours, isRequired: true, requireOnlyOneOf: new []{VehicleUsageColumn.OdometerReading}),
        };

        private static StandardField[] CustomerFields { get; } =
        {
            new StandardField(null, CustomerColumn.IsActive, isRequired: false),
            new StandardField(null, CustomerColumn.Name, isRequired: true),
            new StandardField(null, CustomerColumn.AccountNumber),
            new StandardField(null, CustomerColumn.IsCod),
            new StandardField(null, CustomerColumn.InvoiceEmail),
            new StandardField(null, CustomerColumn.Terms),
            new StandardField(null, CustomerColumn.Address1),
            new StandardField(null, CustomerColumn.Address2),
            new StandardField(null, CustomerColumn.City),
            new StandardField(null, CustomerColumn.State),
            new StandardField(null, CustomerColumn.ZipCode),
            new StandardField(null, CustomerColumn.CountryCode),
            new StandardField(null, CustomerColumn.BillingAddress1),
            new StandardField(null, CustomerColumn.BillingAddress2),
            new StandardField(null, CustomerColumn.BillingCity),
            new StandardField(null, CustomerColumn.BillingState),
            new StandardField(null, CustomerColumn.BillingZipCode),
            new StandardField(null, CustomerColumn.BillingCountryCode),
            new StandardField(null, CustomerColumn.ContactTitle),
            new StandardField(null, CustomerColumn.ContactName),
            new StandardField(null, CustomerColumn.ContactPhone),
            new StandardField(null, CustomerColumn.ContactFax),
            new StandardField(null, CustomerColumn.ContactEmail),
            new StandardField(null, CustomerColumn.Contact2Title),
            new StandardField(null, CustomerColumn.Contact2Name),
            new StandardField(null, CustomerColumn.Contact2Phone),
            new StandardField(null, CustomerColumn.Contact2Email),
        };

        private static StandardField[] EmployeeFields { get; } =
        {
            new StandardField(null, EmployeeColumn.Name, isRequired: true),
            new StandardField(null, EmployeeColumn.Phone),
            new StandardField(null, EmployeeColumn.Email),
            new StandardField(null, EmployeeColumn.Address),
            new StandardField(null, EmployeeColumn.City),
            new StandardField(null, EmployeeColumn.State),
            new StandardField(null, EmployeeColumn.Zip),
            new StandardField(null, EmployeeColumn.SendEmail),
            new StandardField(null, EmployeeColumn.NotifyPreferredFormat),
        };

        private static StandardField[] ItemFields { get; } =
        {
            new StandardField(null, ServiceColumn.IsActive, isRequired: false),
            new StandardField(null, ServiceColumn.Name, isRequired: true),
            new StandardField(null, ServiceColumn.Type, isRequired: true),
            new StandardField(null, ServiceColumn.Description),
            new StandardField(null, ServiceColumn.Uom),
            new StandardField(null, ServiceColumn.Price),
            new StandardField(null, ServiceColumn.IsTaxable),
            new StandardField(null, ServiceColumn.IncomeAccount),
        };

        private static StandardField[] TruckFields { get; } =
        {
            new StandardField(null, TruckColumn.TruckCode, isRequired: true),
            new StandardField(null, TruckColumn.Category),
            new StandardField(null, TruckColumn.CurrentMileage),
            new StandardField(null, TruckColumn.CurrentHours),
            new StandardField(null, TruckColumn.Year),
            new StandardField(null, TruckColumn.Make),
            new StandardField(null, TruckColumn.Model),
            new StandardField(null, TruckColumn.Vin),
            new StandardField(null, TruckColumn.Plate),
            new StandardField(null, TruckColumn.PlateExpiration),
            new StandardField(null, TruckColumn.CargoCapacity),
            new StandardField(null, TruckColumn.CargoCapacityCyds),
            new StandardField(null, TruckColumn.FuelType),
            new StandardField(null, TruckColumn.FuelCapacity),
            new StandardField(null, TruckColumn.SteerTires),
            new StandardField(null, TruckColumn.DriveAxleTires),
            new StandardField(null, TruckColumn.DropAxleTires),
            new StandardField(null, TruckColumn.TrailerTires),
            new StandardField(null, TruckColumn.Transmission),
            new StandardField(null, TruckColumn.Engine),
            new StandardField(null, TruckColumn.RearEnd),
            new StandardField(null, TruckColumn.InsurancePolicyNumber),
            new StandardField(null, TruckColumn.InsuranceValidUntil),
            new StandardField(null, TruckColumn.PurchaseDate),
            new StandardField(null, TruckColumn.PurchasePrice),
            new StandardField(null, TruckColumn.InServiceDate),
            new StandardField(null, TruckColumn.SoldDate),
            new StandardField(null, TruckColumn.SoldPrice),
            new StandardField(null, TruckColumn.Apportioned),
            new StandardField(null, TruckColumn.BedConstruction),
            new StandardField(null, TruckColumn.CanPullTrailer),
            new StandardField(null, TruckColumn.DefaultDriverEmail),
            new StandardField(null, TruckColumn.OutOfService),
            new StandardField(null, TruckColumn.OutOfServiceReason),
            new StandardField(null, TruckColumn.AlwaysShowOnSchedule),
        };

        private static StandardField[] VendorFields { get; } =
        {
            new StandardField(null, VendorColumn.IsActive, isRequired: false),
            new StandardField(null, VendorColumn.Name, isRequired: true),
            new StandardField(null, VendorColumn.MainEmail),
            new StandardField(null, VendorColumn.Address),
            new StandardField(null, VendorColumn.City),
            new StandardField(null, VendorColumn.State),
            new StandardField(null, VendorColumn.ZipCode),
            new StandardField(null, VendorColumn.CountryCode),
            new StandardField(null, VendorColumn.ContactTitle),
            new StandardField(null, VendorColumn.ContactName),
            new StandardField(null, VendorColumn.ContactPhone),
            new StandardField(null, VendorColumn.ContactFax),
            new StandardField(null, VendorColumn.Contact2Name),
            new StandardField(null, VendorColumn.Contact2Phone),
        };

        public class StandardField
        {
            public StandardField(string group, string name, bool allowMulti = false, bool isRequired = false, string[] requireOnlyOneOf = null)
            {
                Group = group;
                Name = name;
                AllowMulti = allowMulti;
                IsRequired = isRequired;
                RequireOnlyOneOf = requireOnlyOneOf;
            }

            public string Group { get; }
            public string Name { get; }
            public bool AllowMulti { get; }
            public bool IsRequired { get; set; }
            public string[] RequireOnlyOneOf { get; set; }
        }
    }
}
