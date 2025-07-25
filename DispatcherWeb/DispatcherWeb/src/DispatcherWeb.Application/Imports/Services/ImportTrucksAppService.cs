using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Timing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.VehicleCategories;
using DispatcherWeb.VehicleMaintenance;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    public class ImportTrucksAppService : ImportDataBaseAppService<TruckImportRow>, IImportTrucksAppService
    {
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private HashSet<string> _existingTruckNames;
        private int? _officeId = null;
        private List<VehicleCategoryDto> _vehicleCategories;
        private List<DriverDto> _driversList;

        public ImportTrucksAppService(
            IRepository<Truck> truckRepository,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IRepository<Driver> driverRepository)
        {
            _truckRepository = truckRepository;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _driverRepository = driverRepository;
        }

        protected override async Task<bool> CacheResourcesBeforeImportAsync(IImportReader reader)
        {
            _existingTruckNames = (await (await _truckRepository.GetQueryAsync()).Select(x => x.TruckCode).ToListAsync()).ToHashSet();

            _officeId = await OfficeResolver.GetOfficeIdAsync(_userId.ToString());
            if (_officeId == null)
            {
                _result.NotFoundOffices.Add(_userId.ToString());
                return false;
            }

            _vehicleCategories = await (await _vehicleCategoryRepository.GetQueryAsync())
                .Select(x => new VehicleCategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    AssetType = x.AssetType,
                }).ToListAsync();

            _driversList = await (await _driverRepository.GetQueryAsync())
                .Select(x => new DriverDto
                {
                    Id = x.Id,
                    EmailAddress = x.EmailAddress,
                }).ToListAsync();

            return await base.CacheResourcesBeforeImportAsync(reader);
        }

        protected override async Task<bool> ImportRowAsync(TruckImportRow row)
        {
            if (_existingTruckNames.Contains(row.TruckCode))
            {
                return false;
            }

            var vehicleCategory = GetVehicleCategory(row);
            if (vehicleCategory == null)
            {
                return false;
            }

            var truck = new Truck
            {
                TruckCode = row.TruckCode,
                VehicleCategoryId = vehicleCategory.Id,
                OfficeId = _officeId,
                IsActive = true,
                CurrentMileage = row.CurrentMileage,
                CurrentHours = row.CurrentHours,
                Year = row.Year,
                Make = row.Make,
                Model = row.Model,
                Vin = row.Vin,
                Plate = row.Plate,
                PlateExpiration = row.PlateExpiration,
                CargoCapacity = row.CargoCapacity,
                CargoCapacityCyds = row.CargoCapacityCyds,
                FuelType = row.FuelType,
                FuelCapacity = row.FuelCapacity,
                SteerTires = row.SteerTires,
                DriveAxleTires = row.DriveAxleTires,
                DropAxleTires = row.DropAxleTires,
                TrailerTires = row.TrailerTires,
                Transmission = row.Transmission,
                Engine = row.Engine,
                RearEnd = row.RearEnd,
                InsurancePolicyNumber = row.InsurancePolicyNumber,
                InsuranceValidUntil = row.InsuranceValidUntil,
                PurchaseDate = row.PurchaseDate,
                PurchasePrice = row.PurchasePrice,
                InServiceDate = row.InServiceDate ?? Clock.Now.ConvertTimeZoneTo(_timeZone).Date,
                SoldDate = row.SoldDate,
                SoldPrice = row.SoldPrice,
                IsApportioned = row.Apportioned,
                BedConstruction = row.BedConstruction,
                CanPullTrailer = GetCanPullTrailer(row.CanPullTrailer, vehicleCategory.AssetType),
                DefaultDriverId = GetDriverId(row.DefaultDriverEmail),
                IsOutOfService = row.OutOfService,
                AlwaysShowOnSchedule = row.AlwaysShowOnSchedule,
                OutOfServiceHistories = GetOutOfServiceReason(row.OutOfService, row.OutOfServiceReason),
            };
            await _truckRepository.InsertAsync(truck);

            _existingTruckNames.Add(row.TruckCode);
            return true;
        }

        private static List<OutOfServiceHistory> GetOutOfServiceReason(bool isOutOfService, string reason)
        {
            if (!isOutOfService)
            {
                return null;
            }

            return new List<OutOfServiceHistory>
            {
                new OutOfServiceHistory
                {
                    OutOfServiceDate = Clock.Now,
                    Reason = reason.Truncate(500),
                },
            };
        }

        private static bool GetCanPullTrailer(bool? canPullTrailer, AssetType assetType)
        {
            return canPullTrailer ?? assetType == AssetType.Tractor;
        }

        private int? GetDriverId(string driverEmail)
        {
            if (driverEmail.IsNullOrEmpty())
            {
                return null;
            }

            var driver = _driversList.FirstOrDefault(x => driverEmail.Equals(x.EmailAddress, StringComparison.OrdinalIgnoreCase));
            return driver?.Id;
        }


        [CanBeNull]
        private VehicleCategoryDto GetVehicleCategory(TruckImportRow row)
        {
            VehicleCategoryDto vehicleCategory;
            var name = row.VehicleCategoryName;

            if (string.IsNullOrEmpty(name))
            {
                vehicleCategory = _vehicleCategories.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).FirstOrDefault();
                if (vehicleCategory == null)
                {
                    row.AddParseErrorIfNotExist(Columns.TruckColumn.Category, "No vehicle categories were found in DB to fallback on",
                        typeof(string));
                    return null;
                }

                return vehicleCategory;
            }

            vehicleCategory = _vehicleCategories.FirstOrDefault(x => name.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            if (vehicleCategory != null)
            {
                return vehicleCategory;
            }

            if (name.ToLower().EndsWith('s'))
            {
                vehicleCategory = _vehicleCategories.FirstOrDefault(x => name.TrimEnd('s').Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                if (vehicleCategory != null)
                {
                    return vehicleCategory;
                }
            }

            row.AddParseErrorIfNotExist(Columns.TruckColumn.Category, name, typeof(string));
            return null;
        }

        protected override bool IsRowEmpty(TruckImportRow row)
        {
            return row.TruckCode.IsNullOrEmpty();
        }
    }
}
