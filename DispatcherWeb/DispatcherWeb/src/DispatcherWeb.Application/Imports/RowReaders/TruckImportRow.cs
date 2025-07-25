using System;
using System.Linq;
using CsvHelper;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Imports.RowReaders
{
    public class TruckImportRow : ImportRow
    {
        public TruckImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public string TruckCode => GetString(TruckColumn.TruckCode, Truck.MaxTruckCodeLength);

        //public bool IsActive => GetString(TruckColumn.IsActive, 30) == "Active" || !HasField(TruckColumn.IsActive);
        //public DateTime? InactivationDate
        //public bool IsOutOfService
        public decimal CurrentMileage => GetDecimal(TruckColumn.CurrentMileage) ?? 0;

        public decimal CurrentHours => GetDecimal(TruckColumn.CurrentHours) ?? 0;

        public int? Year => (int?)GetDecimal(TruckColumn.Year, 0);

        public string Make => GetString(TruckColumn.Make, EntityStringFieldLengths.Truck.Make);

        public string Model => GetString(TruckColumn.Model, EntityStringFieldLengths.Truck.Model);

        public string Vin => GetString(TruckColumn.Vin, EntityStringFieldLengths.Truck.Vin);

        public string Plate => GetString(TruckColumn.Plate, EntityStringFieldLengths.Truck.Plate);

        public DateTime? PlateExpiration => GetDate(TruckColumn.PlateExpiration);

        public decimal? CargoCapacity => GetDecimal(TruckColumn.CargoCapacity);

        public decimal? CargoCapacityCyds => GetDecimal(TruckColumn.CargoCapacityCyds);

        public int? FuelCapacity => (int?)GetDecimal(TruckColumn.FuelCapacity, 0);

        public string SteerTires => GetString(TruckColumn.SteerTires, EntityStringFieldLengths.Truck.SteerTires);

        public string DriveAxleTires => GetString(TruckColumn.DriveAxleTires, EntityStringFieldLengths.Truck.DriveAxleTires);

        public string DropAxleTires => GetString(TruckColumn.DropAxleTires, EntityStringFieldLengths.Truck.DropAxleTires);

        public string TrailerTires => GetString(TruckColumn.TrailerTires, EntityStringFieldLengths.Truck.TrailerTires);

        public string Transmission => GetString(TruckColumn.Transmission, EntityStringFieldLengths.Truck.Transmission);

        public string Engine => GetString(TruckColumn.Engine, EntityStringFieldLengths.Truck.Engine);

        public string RearEnd => GetString(TruckColumn.RearEnd, EntityStringFieldLengths.Truck.RearEnd);

        public string InsurancePolicyNumber => GetString(TruckColumn.InsurancePolicyNumber, EntityStringFieldLengths.Truck.InsurancePolicyNumber);

        public DateTime? InsuranceValidUntil => GetDate(TruckColumn.InsuranceValidUntil);

        public DateTime? PurchaseDate => GetDate(TruckColumn.PurchaseDate);

        public decimal? PurchasePrice => GetDecimal(TruckColumn.PurchasePrice);

        public DateTime? InServiceDate => GetDate(TruckColumn.InServiceDate);

        public DateTime? SoldDate => GetDate(TruckColumn.SoldDate);

        public decimal? SoldPrice => GetDecimal(TruckColumn.SoldPrice);

        public string VehicleCategoryName => GetString(TruckColumn.Category, 100);

        public string OutOfServiceReason => GetString(TruckColumn.OutOfServiceReason, EntityStringFieldLengths.General.Length2000);

        public string DefaultDriverEmail => GetString(TruckColumn.DefaultDriverEmail, EntityStringFieldLengths.Driver.EmailAddress);

        public FuelType? FuelType
        {
            get
            {
                var fuelTypeString = GetString(TruckColumn.FuelType, 100);

                if (!string.IsNullOrEmpty(fuelTypeString) && Utilities.TryGetEnumFromDisplayName<FuelType>(fuelTypeString, out var fuelType))
                {
                    return fuelType;
                }

                return null;
            }
        }

        public BedConstructionEnum? BedConstruction
        {
            get
            {
                var bedConstructionString = GetString(TruckColumn.BedConstruction, 100);

                if (!string.IsNullOrEmpty(bedConstructionString) && Utilities.TryGetEnumFromDisplayName<BedConstructionEnum>(bedConstructionString,
                        out var bedConstructionType))
                {
                    return bedConstructionType;
                }

                return null;
            }
        }

        public bool Apportioned => GetBoolean(TruckColumn.Apportioned, false);

        public bool AlwaysShowOnSchedule => GetBoolean(TruckColumn.AlwaysShowOnSchedule, true);

        public bool? CanPullTrailer => GetNullableBoolean(TruckColumn.CanPullTrailer);

        public bool OutOfService => GetBoolean(TruckColumn.OutOfService);
    }
}
