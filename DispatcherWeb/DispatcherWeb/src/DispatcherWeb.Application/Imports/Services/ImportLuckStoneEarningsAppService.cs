using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.Locations;
using DispatcherWeb.LuckStone;
using DispatcherWeb.Orders;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    [AbpAuthorize(AppPermissions.Pages_Imports_Tickets_IronSheepdogEarnings)]
    public class ImportLuckStoneEarningsAppService : ImportTicketEarningsBaseAppService, IImportLuckStoneEarningsAppService
    {
        private readonly IRepository<LuckStoneLocation> _luckStoneLocationRepository;

        public ImportLuckStoneEarningsAppService(
            IRepository<ImportedEarnings> importedEarningsRepository,
            IRepository<ImportedEarningsBatch> importedEarningsBatchRepository,
            IRepository<LuckStoneLocation> luckStoneLocationRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Customer> customerRepository,
            IRepository<UnitOfMeasure> uomRepository,
            IRepository<Item> itemRepository,
            IRepository<Location> locationRepository,
            IRepository<LocationCategory> locationCategoryRepository,
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            ISecureFileBlobService secureFileBlobService,
            UserManager userManager
        ) : base(
            importedEarningsRepository,
            importedEarningsBatchRepository,
            orderRepository,
            orderLineRepository,
            orderLineTruckRepository,
            driverAssignmentRepository,
            availableLeaseHaulerTruckRepository,
            ticketRepository,
            employeeTimeRepository,
            customerRepository,
            uomRepository,
            itemRepository,
            locationRepository,
            locationCategoryRepository,
            truckRepository,
            driverRepository,
            timeClassificationRepository,
            secureFileBlobService,
            userManager
        )
        {
            _luckStoneLocationRepository = luckStoneLocationRepository;
        }

        protected override string GetExpectedCsvHeader()
        {
            return "Haultickets_TicketDateTime,Haultickets_HaulerRef,Haultickets_Licenseplate,Haultickets_Site,Haultickets_ProductDescription,Haultickets_CustomerName,Haultickets_TicketID,Haultickets_HaulPaymentRate,Haultickets_HaulPaymentRateUOM,Haultickets_NetTons,Haultickets_FSCAmount,Haultickets_HaulPayment";
        }

        protected override TicketImportType TicketImportType => TicketImportType.LuckStone;

        protected override string ImportFileDisplayName => "Luck Stone Hauling";

        protected override async Task<bool> GetProductionPayValue()
        {
            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.LuckStone.UseForProductionPay);
        }

        protected override async Task<int> GetCustomerId()
        {
            return await SettingManager.GetSettingValueAsync<int>(AppSettings.LuckStone.LuckStoneCustomerId);
        }

        protected override async Task<string> GetHaulerRef()
        {
            return await SettingManager.GetSettingValueAsync(AppSettings.LuckStone.HaulerRef);
        }

        protected override TicketImportTruckMatching TruckMatching => TicketImportTruckMatching.ByLicensePlate;

        protected override bool AreRequiredFieldsFilled(TicketEarningsImportRow row)
        {
            return !string.IsNullOrEmpty(row.TicketNumber)
                && row.TicketDateTime.HasValue
                && !string.IsNullOrEmpty(row.Site)
                && !string.IsNullOrEmpty(row.CustomerName)
                && !string.IsNullOrEmpty(row.LicensePlate)
                && row.HaulPaymentRate.HasValue
                && row.NetTons.HasValue
                && row.HaulPayment.HasValue
                && !string.IsNullOrEmpty(row.HaulerRef)
                && row.FscAmount.HasValue
                && !string.IsNullOrEmpty(row.HaulPaymentRateUom)
                && !string.IsNullOrEmpty(row.ProductDescription);
        }

        protected override async Task PopulateLoadAtLocationsFromSites(List<string> sites)
        {
            _loadAtLocations ??= new();

            var locationGroups = await (
                from luckStoneLocation in (await _luckStoneLocationRepository.GetQueryAsync())
                    .Where(x => sites.Contains(x.Site))
                join existingLocation in await _locationRepository.GetQueryAsync()
                    on new { luckStoneLocation.Name, luckStoneLocation.StreetAddress, luckStoneLocation.City, luckStoneLocation.State, luckStoneLocation.ZipCode }
                    equals new { existingLocation.Name, existingLocation.StreetAddress, existingLocation.City, existingLocation.State, existingLocation.ZipCode }
                    into existingLocationLeftJoin
                from existingLocation in existingLocationLeftJoin.DefaultIfEmpty()

                select new
                {
                    LuckStoneLocation = new
                    {
                        luckStoneLocation.Site,
                        luckStoneLocation.Name,
                        luckStoneLocation.StreetAddress,
                        luckStoneLocation.City,
                        luckStoneLocation.State,
                        luckStoneLocation.ZipCode,
                        luckStoneLocation.CountryCode,
                        luckStoneLocation.Latitude,
                        luckStoneLocation.Longitude,
                    },
                    ExistingLocation = existingLocation == null ? null : new
                    {
                        existingLocation.Id,
                    },
                }
                ).ToListAsync();

            foreach (var site in sites.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var locationGroup = locationGroups.FirstOrDefault(x => x.LuckStoneLocation.Site.ToLower() == site.ToLower());
                if (locationGroup != null)
                {
                    if (locationGroup.ExistingLocation != null)
                    {
                        _loadAtLocations.Add(site.ToLower(), locationGroup.ExistingLocation.Id);
                    }
                    else
                    {
                        var location = new Location
                        {
                            Name = locationGroup.LuckStoneLocation.Name,
                            StreetAddress = locationGroup.LuckStoneLocation.StreetAddress,
                            City = locationGroup.LuckStoneLocation.City,
                            State = locationGroup.LuckStoneLocation.State,
                            ZipCode = locationGroup.LuckStoneLocation.ZipCode,
                            CountryCode = locationGroup.LuckStoneLocation.CountryCode,
                            Latitude = locationGroup.LuckStoneLocation.Latitude,
                            Longitude = locationGroup.LuckStoneLocation.Longitude,
                            IsActive = true,
                        };
                        await _locationRepository.InsertAndGetIdAsync(location);
                        _loadAtLocations.Add(site.ToLower(), location.Id);
                    }
                }
                else
                {
                    var locationName = "Luck Stone " + site;
                    var location = await (await _locationRepository.GetQueryAsync())
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Name == locationName);

                    if (location == null)
                    {
                        location = new Location
                        {
                            Name = locationName,
                            IsActive = true,
                        };
                        await _locationRepository.InsertAndGetIdAsync(location);
                    }

                    _loadAtLocations.Add(site.ToLower(), location.Id);
                }
            }
        }
    }
}
